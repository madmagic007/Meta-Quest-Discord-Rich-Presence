package com.madmagic.mqrpc.receivers;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import com.madmagic.mqrpc.main.MainService;

public class BootReceiver extends BroadcastReceiver {

    @Override
    public void onReceive(Context context, Intent intent) {
        context.startForegroundService(new Intent(context.getApplicationContext(), MainService.class));
    }
}
