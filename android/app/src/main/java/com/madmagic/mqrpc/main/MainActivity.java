package com.madmagic.mqrpc.main;

import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;
import android.provider.Settings;
import android.util.Log;
import android.view.View;
import android.widget.*;
import androidx.appcompat.app.AppCompatActivity;
import com.madmagic.mqrpc.*;
import com.madmagic.mqrpc.api.ApiSender;
import com.madmagic.mqrpc.utils.ConnectionChecker;
import com.madmagic.mqrpc.utils.DeviceInfo;
import com.madmagic.mqrpc.utils.PermissionHandler;

import java.io.File;
import java.io.FileWriter;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

public class MainActivity extends AppCompatActivity {

    public static boolean b;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        b = true;

        Config.init(this);
        PermissionHandler.checkAndPromptBatteryPermission(this, false);
        PermissionHandler.checkUsageStatsPermission(this, false);

        Intent service = new Intent(this, MainService.class);
        if (!MainService.isRunning(getApplicationContext(), MainService.class)) {
            startForegroundService(service);
        }

        findViewById(R.id.btnStop).setOnClickListener(v -> {
            if (MainService.isRunning(getApplicationContext(), MainService.class)) {
                stopService(service);
                ConnectionChecker.end();
            }
        });

        findViewById(R.id.btnStart).setOnClickListener(v -> {
            if (!MainService.isRunning(getApplicationContext(), MainService.class)) {
                startService(service);
            }
        });

        findViewById(R.id.btnLog).setOnClickListener(v -> new Thread(() -> {
            try {
                File logFile = new File(getExternalFilesDir(null), "log.txt");
                if (!logFile.exists()) logFile.createNewFile();

                String log = Config.config.toString(4) + "\n" +
                        "Trying to request connection: " +
                        ApiSender.send("connect", MainService.getIp(getBaseContext())).toString() +
                        "\ncurrent top: " + Arrays.toString(DeviceInfo.getTopmost(getBaseContext()));

                FileWriter fw = new FileWriter(logFile);
                fw.write(log);
                fw.close();
            } catch (Exception e) {
                Log.d("MQRPC", Log.getStackTraceString(e));
            }
        }).start());

        findViewById(R.id.btnPermissions).setOnClickListener(v -> {
            PermissionHandler.checkAndPromptBatteryPermission(this, true);
            PermissionHandler.checkUsageStatsPermission(this, true);
        });

        ((TextView) findViewById(R.id.ipv4Field)).setText(MainService.getIp(this));

        TextView moduleState = findViewById(R.id.txtModuleEnabled);

        List<String> list = new ArrayList<>(Module.modules.keySet());
        if (list.isEmpty()) list.add("No modules found");
        else list.add(0, "Modules");

        ArrayAdapter<String> adapter = new ArrayAdapter<>(this, android.R.layout.simple_spinner_item, list);
        adapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);

        Spinner spinner = findViewById(R.id.modulesSpinner);
        spinner.setAdapter(adapter);

        Button btnEnable = findViewById(R.id.btnEnable);
        btnEnable.setOnClickListener(v -> updateModule((String) spinner.getSelectedItem(), true, moduleState));

        Button btnDisable = findViewById(R.id.btnDisable);
        btnDisable.setOnClickListener(v -> updateModule((String) spinner.getSelectedItem(), false, moduleState));

        spinner.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
            @Override
            public void onItemSelected(AdapterView<?> adapterView, View view, int i, long l) {
                String packageName = (String) spinner.getSelectedItem();

                if (packageName.equals("No modules found") || packageName.equals("Modules")) {
                    btnDisable.setVisibility(View.INVISIBLE);
                    btnEnable.setVisibility(View.INVISIBLE);
                    moduleState.setVisibility(View.INVISIBLE);
                } else {
                    btnDisable.setVisibility(View.VISIBLE);
                    btnEnable.setVisibility(View.VISIBLE);
                    moduleState.setVisibility(View.VISIBLE);

                    boolean enabled = Module.isEnabled(packageName);
                    moduleState.setText(enabled ? "Enabled" : "Disabled");
                }
            }

            @Override
            public void onNothingSelected(AdapterView<?> adapterView) {}
        });
    }

    private static void updateModule(String packageName, boolean enabled, TextView text) {
        Module module = Module.modules.get(packageName);
        if (module == null) return;

        module.enabled = enabled;
        Module.modules.put(packageName, module);
        Config.updateModules(Module.modules);
        text.setText(enabled ? "Enabled" : "Disabled");
    }

    @Override
    protected void onDestroy() {
        b = false;
        super.onDestroy();
    }
}
