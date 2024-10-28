package com.madmagic.mqrpc.utils;

import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.pm.ApplicationInfo;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Canvas;
import android.graphics.drawable.BitmapDrawable;
import android.graphics.drawable.Drawable;
import android.os.BatteryManager;
import android.provider.Settings;
import android.util.Base64;
import android.util.Log;
import com.madmagic.mqrpc.Config;
import com.madmagic.mqrpc.main.MainService;
import net.dongliu.apk.parser.ApkFile;
import net.dongliu.apk.parser.bean.ApkMeta;
import org.json.JSONArray;
import org.json.JSONObject;

import java.io.ByteArrayOutputStream;
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
        String[] data = new String[] {ForegroundChecker.getForegroundApp(c), "", ""};

        try {
            ApplicationInfo ai = c.getPackageManager().getApplicationInfo(data[0], 0);
            String apkPath = ai.publicSourceDir;

            try (ApkFile apkFile = new ApkFile(new File(apkPath))) {
                ApkMeta meta = apkFile.getApkMeta();
                data[1] = meta.getName();
            }

            //data[2] = getIconBase64(apkPath, c);

        } catch (Exception e) {
            Log.d("MQRPC", Log.getStackTraceString(e));
        }

        return data;
    }

    private static String getIconBase64(String apkPath, Context c) {
        PackageManager pm = c.getPackageManager();
        PackageInfo pi = pm.getPackageArchiveInfo(apkPath, 0);
        pi.applicationInfo.sourceDir = apkPath;
        pi.applicationInfo.publicSourceDir = apkPath;

        return getBase64FromDrawable(pi.applicationInfo.loadIcon(pm));
    }

    private static String getBase64FromDrawable(Drawable icon) {
        Bitmap bitmap = drawableToBitmap(icon);

        try (ByteArrayOutputStream baos = new ByteArrayOutputStream()) {
            bitmap.compress(Bitmap.CompressFormat.JPEG, 100, baos);
            byte[] byteArray = baos.toByteArray();

            return Base64.encodeToString(byteArray, Base64.DEFAULT);
        } catch (Exception ignored) {}
        return "";
    }

    private static Bitmap drawableToBitmap(Drawable drawable) {
        Bitmap bitmap;

        if (drawable instanceof BitmapDrawable) {
            BitmapDrawable bitmapDrawable = (BitmapDrawable) drawable;
            if(bitmapDrawable.getBitmap() != null) {
                return bitmapDrawable.getBitmap();
            }
        }

        if(drawable.getIntrinsicWidth() <= 0 || drawable.getIntrinsicHeight() <= 0) {
            bitmap = Bitmap.createBitmap(1, 1, Bitmap.Config.ARGB_8888); // Single color bitmap will be created of 1x1 pixel
        } else {
            bitmap = Bitmap.createBitmap(drawable.getIntrinsicWidth(), drawable.getIntrinsicHeight(), Bitmap.Config.ARGB_8888);
        }

        Canvas canvas = new Canvas(bitmap);
        drawable.setBounds(0, 0, canvas.getWidth(), canvas.getHeight());
        drawable.draw(canvas);
        return bitmap;
    }
}
