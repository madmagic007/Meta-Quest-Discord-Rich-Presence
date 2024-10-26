package com.madmagic.mqrpc.utils;

import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.pm.ApplicationInfo;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.os.BatteryManager;
import android.provider.Settings;
import android.util.Log;
import com.madmagic.mqrpc.Config;
import com.madmagic.mqrpc.main.MainService;
import net.dongliu.apk.parser.ApkFile;
import net.dongliu.apk.parser.bean.ApkMeta;
import org.json.JSONArray;
import org.json.JSONObject;

import java.io.File;

public class DeviceInfo {

    public static String getInfo(MainService s) {
        try {
            JSONObject r = new JSONObject();

            Intent batteryIntent = s.getBaseContext().getApplicationContext().registerReceiver(null, new IntentFilter(Intent.ACTION_BATTERY_CHANGED));
            int level = batteryIntent.getIntExtra(BatteryManager.EXTRA_LEVEL, -1);
            r.put("batteryLevel", level + "%");

            r.put("name", getName(s));

            String[] topMost = getTopmost(s);
            r.put("currentTop", new JSONArray().put(topMost[0]).put(topMost[1]))
                    .put("questAddress", MainService.getIp(s))
                    .put("pcAddress", Config.getAddress().isEmpty() ? "not set" : Config.getAddress());

            return r.toString(4);
        } catch (Exception ignored) {
            return "";
        }
    }

    public static String getName(Context c) {
        return Settings.Global.getString(c.getContentResolver(), "device_name");
    }

    public static String[] getTopmost(Context c) {
        String[] data = new String[] {ForegroundChecker.getForegroundApp(c), ""};

        try {
            ApplicationInfo ai = c.getPackageManager().getApplicationInfo(data[0], 0);

            try (ApkFile apkFile = new ApkFile(new File(ai.publicSourceDir))) {
                ApkMeta meta = apkFile.getApkMeta();
                data[1] = meta.getName();
            }
        } catch (Exception e) {
            Log.d("MQRPC", Log.getStackTraceString(e));
        }

        return data;
    }
}
