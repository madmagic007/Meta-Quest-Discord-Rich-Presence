package com.madmagic.mqrpc.utils;

import android.app.AppOpsManager;
import android.content.Context;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.net.Uri;
import android.os.PowerManager;
import android.provider.Settings;
import android.util.Log;

public class PermissionHandler {

    public static void checkUsageStatsPermission(Context c, boolean force) {
        if (hasUsageStatsPermission(c) && !force) return;

        Intent grantPermission = new Intent(Settings.ACTION_USAGE_ACCESS_SETTINGS);
        grantPermission.setData(Uri.fromParts("package", c.getPackageName(), null));
        c.startActivity(grantPermission);
    }

    public static boolean hasUsageStatsPermission(Context c) {
        AppOpsManager appOps = (AppOpsManager) c.getSystemService(Context.APP_OPS_SERVICE);
        int mode = appOps.unsafeCheckOpNoThrow(AppOpsManager.OPSTR_GET_USAGE_STATS, android.os.Process.myUid(), c.getPackageName());

        if (mode == AppOpsManager.MODE_DEFAULT) {
            return c.checkCallingOrSelfPermission(android.Manifest.permission.PACKAGE_USAGE_STATS) == PackageManager.PERMISSION_GRANTED;
        } else {
            return mode == AppOpsManager.MODE_ALLOWED;
        }
    }

    public static void checkAndPromptBatteryPermission(Context c, boolean force) {
        String packageName = c.getPackageName();
        PowerManager pm = (PowerManager) c.getSystemService(Context.POWER_SERVICE);

        if (pm.isIgnoringBatteryOptimizations(packageName) && !force) return;

        Log.d("MQRPC", "battery: " + packageName);
        Intent intent = new Intent();
        intent.setAction(Settings.ACTION_REQUEST_IGNORE_BATTERY_OPTIMIZATIONS);
        intent.setData(Uri.parse("package:" + packageName));
        c.startActivity(intent);
    }
}
