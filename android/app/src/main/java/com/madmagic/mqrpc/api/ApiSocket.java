package com.madmagic.mqrpc.api;

import android.util.Log;
import com.madmagic.mqrpc.Config;
import com.madmagic.mqrpc.Module;
import com.madmagic.mqrpc.main.MainService;
import com.madmagic.mqrpc.utils.DeviceInfo;
import fi.iki.elonen.NanoHTTPD;
import net.dongliu.apk.parser.Main;
import org.json.JSONObject;

import java.io.IOException;
import java.util.HashMap;

public class ApiSocket extends NanoHTTPD {

    private final MainService s;

    public ApiSocket(MainService s) throws IOException {
        super(16255);
        start(NanoHTTPD.SOCKET_READ_TIMEOUT, false);
        this.s = s;
    }

    @Override
    public Response serve(IHTTPSession session) {
        String r = "{}";

        try {
            final HashMap<String, String> map = new HashMap<>();
            session.parseBody(map);

            if (!map.containsKey("postData")) {
                return newFixedLengthResponse(DeviceInfo.getInfo(s));
            }

            JSONObject pO = new JSONObject(map.get("postData"));
            if (pO.has("pcAddress"))
                Config.setAddress(pO.getString("pcAddress"));
            if (pO.has("sleepWake"))
                Config.setSleepWake(pO.getBoolean("sleepWake"));

            switch (pO.getString("message")) {

                case "game":
                    String[] topmostData = DeviceInfo.getTopmost(s.getBaseContext());
                    String packageName = topmostData[0];
                    JSONObject response = new JSONObject();

                    boolean detailed = false;
                    if (Module.isEnabled(packageName)) {
                        int port = Module.getPort(packageName);
                        response = ApiSender.moduleSocket(port);
                        detailed = !response.toString().equals("{}");

                        String appId = Module.getAppKey(packageName);
                        if (!appId.isEmpty()) response.put("appId", appId);
                    }
                    r = response.put("message", "gameResponse")
                            .put("packageName", packageName)
                            .put("name", topmostData[1])
                            //.put("apkIcon", topmostData[2])
                            .put("detailed", detailed)
                            .toString();
                    break;

                case "startup":
                    new Thread(s::callStart).start();
                    break;

                case "validate":
                    r = new JSONObject()
                            .put("valid", "You just found a secret :)")
                            .toString();
                    break;
            }

        } catch (Exception e) {
            Log.d("MQRPC", Log.getStackTraceString(e));
        }

        return newFixedLengthResponse(r);
    }
}