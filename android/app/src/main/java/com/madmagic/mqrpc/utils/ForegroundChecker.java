package com.madmagic.mqrpc.utils;

import android.app.AppOpsManager;
import android.app.Service;
import android.app.usage.UsageEvents;
import android.app.usage.UsageStatsManager;
import android.content.Context;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.net.Uri;
import android.os.PowerManager;
import android.provider.Settings;

public class ForegroundChecker {

    public static String getForegroundApp(Context c) {
        if(!PermissionHandler.hasUsageStatsPermission(c)) return "";

        UsageStatsManager mUsageStatsManager = (UsageStatsManager) c.getSystemService(Service.USAGE_STATS_SERVICE);
        long time = System.currentTimeMillis();

        UsageEvents usageEvents = mUsageStatsManager.queryEvents(time - 1000 * 3600, time);
        UsageEvents.Event event = new UsageEvents.Event();

        String foregroundApp = "";
        while (usageEvents.hasNextEvent()) {
            usageEvents.getNextEvent(event);

            if(event.getEventType() != UsageEvents.Event.MOVE_TO_FOREGROUND) continue;

            foregroundApp = event.getPackageName();
        }

        return foregroundApp;
    }


}
