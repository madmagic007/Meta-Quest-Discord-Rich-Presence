package com.madmagic.mqrpc.receivers;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.util.Log;
import com.madmagic.mqrpc.Config;
import com.madmagic.mqrpc.main.MainService;

public class ScreenReceiver extends BroadcastReceiver {

    @Override
    public void onReceive(Context context, Intent intent) {
        Log.d("MQRPC", "received screen");
        switch (intent.getAction()) {

            case Intent.ACTION_SCREEN_OFF:
                if (MainService.isRunning(context, MainService.class) && Config.getSleepWake()) {
                    context.stopService(new Intent(context, MainService.class));
                }
                break;

            case Intent.ACTION_SCREEN_ON:
                if (!MainService.isRunning(context, MainService.class) && Config.getSleepWake())
                    context.startForegroundService(new Intent(context, MainService.class));
                break;
        }
    }

    private static boolean registered = false;

    public static void register(Context context) {
        if (registered) return;
        registered = true;

        IntentFilter filter = new IntentFilter();
        filter.addAction(Intent.ACTION_SCREEN_OFF);
        filter.addAction(Intent.ACTION_SCREEN_ON);

        context.registerReceiver(new ScreenReceiver(), filter);
    }
}
