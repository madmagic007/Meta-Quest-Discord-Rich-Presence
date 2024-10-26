package com.madmagic.mqrpc;

import android.app.AppOpsManager;
import android.app.Service;
import android.app.usage.UsageEvents;
import android.app.usage.UsageStatsManager;
import android.content.Context;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.net.Uri;
import android.provider.Settings;

public class ForegroundChecker {

    public static String getForegroundApp(Context c) {
        if(!hasUsageStatsPermission(c)) return "";

        String foregroundApp = null;

        UsageStatsManager mUsageStatsManager = (UsageStatsManager) c.getSystemService(Service.USAGE_STATS_SERVICE);
        long time = System.currentTimeMillis();

        UsageEvents usageEvents = mUsageStatsManager.queryEvents(time - 1000 * 3600, time);
        UsageEvents.Event event = new UsageEvents.Event();

        while (usageEvents.hasNextEvent()) {
            usageEvents.getNextEvent(event);

            if(event.getEventType() != UsageEvents.Event.MOVE_TO_FOREGROUND) continue;

            foregroundApp = event.getPackageName();
        }

        return foregroundApp ;
    }

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
}
