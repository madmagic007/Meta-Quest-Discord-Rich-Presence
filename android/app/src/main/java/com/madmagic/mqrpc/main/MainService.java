package com.madmagic.mqrpc.main;

import android.app.*;
import android.content.Context;
import android.content.Intent;

import android.graphics.drawable.Icon;
import android.net.wifi.WifiManager;
import android.os.IBinder;
import android.text.format.Formatter;
import com.madmagic.mqrpc.Config;
import com.madmagic.mqrpc.R;
import com.madmagic.mqrpc.api.ApiSender;
import com.madmagic.mqrpc.api.ApiSocket;
import com.madmagic.mqrpc.receivers.ScreenReceiver;
import com.madmagic.mqrpc.utils.ConnectionChecker;
import com.madmagic.mqrpc.utils.PermissionHandler;


public class MainService extends Service {

    private ApiSocket receiver;
    @Override
    public void onCreate() {
        super.onCreate();

        try {
            receiver = new ApiSocket(this);

            if (!PermissionHandler.hasUsageStatsPermission(this) && !MainActivity.b) {
                stopSelf();
                System.exit(0);
            }

            Config.init(this);
            if (!Config.getAddress().isEmpty()) {
                ConnectionChecker.run(this);
            }
        } catch (Exception ignored) {}

        startForeground(1, createNotification());
        ScreenReceiver.register(getApplicationContext());
    }

    public void callStart() {
        ApiSender.send("online", getIp(this));
    }

    @Override
    public void onDestroy() {
        new Thread(() -> ApiSender.send("offline", getIp(getBaseContext()))).start();
        if (receiver.isAlive()) receiver.stop();
    }

    public static String getIp(Context context) {
        String ip = "";
        try {
            WifiManager wm = (WifiManager) context.getApplicationContext().getSystemService(WIFI_SERVICE);
            ip = Formatter.formatIpAddress(wm.getConnectionInfo().getIpAddress());
        } catch (Exception ignored) {}
        return ip;
    }

    public static boolean isRunning(Context context, Class<?> serviceClass) {
        ActivityManager manager = (ActivityManager) context.getSystemService(Context.ACTIVITY_SERVICE);
        for (ActivityManager.RunningServiceInfo service : manager.getRunningServices(Integer.MAX_VALUE)) {
            if (serviceClass.getName().equals(service.service.getClassName())) {
                return true;
            }
        }
        return false;
    }

    @Override
    public int onStartCommand(Intent intent, int flags, int startId) {
        return START_STICKY;
    }

    @Override
    public IBinder onBind(Intent intent) {
        return null;
    }


    private Notification createNotification() {
        String appName = getString(R.string.appName);

        NotificationChannel channel = new NotificationChannel(appName,
                appName, NotificationManager.IMPORTANCE_LOW);

        NotificationManager manager = getSystemService(NotificationManager.class);

        if (manager != null) {
            manager.createNotificationChannel(channel);
        }

        return new Notification.Builder(this, appName)
                .setContentTitle(appName)
                .setContentText(getString(R.string.notifTitle))
                .build();
    }
}