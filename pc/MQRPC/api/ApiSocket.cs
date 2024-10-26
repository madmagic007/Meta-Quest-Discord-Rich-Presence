﻿using Newtonsoft.Json.Linq;
using NHttp;
using MQRPC.presence;
using MQRPC.settings;
using System.Net;

namespace MQRPC.api {

    class ApiSocket : HttpServer {

        public static int port = 16255;

        public ApiSocket() {
            EndPoint = new IPEndPoint(IPAddress.Any, port);
            Console.WriteLine("Started listening at port: " + port);
            Start();
        }

        protected override void OnRequestReceived(HttpRequestEventArgs e) {
            JObject resp = new JObject();
            if (e.Request.InputStream == null) {
                WriteResponse(e.Response, "Yep, your pc is running");
                return;
            }
            using StreamReader sr = new(e.Request.InputStream);
            string msg = sr.ReadToEnd();
            Console.WriteLine(msg);
            JObject obj = JObject.Parse(msg);

            bool update = false;
            if (obj.ContainsKey("apkVersion")) {
                Config.cfg.apkVersion = (string)obj["apkVersion"];
                update = true;
            }

            if (obj.ContainsKey("questAddress")) {
                Config.cfg.address = (string)obj["questAddress"];
                update = true;
            }

            if (update) Config.Save();

            switch ((string)obj["message"]) {
                case "online":
                    Program.SendNotif("Presence on your quest has started");
                    Timers.StartRequesting();
                    DiscordHandler.Init();
                    break;
                case "offline":
                    PresenceHandler.Stop();
                    break;
                case "connect":
                    resp["connected"] = "sucesfully connected";
                    break;
            }

            WriteResponse(e.Response, resp.ToString());
        }

        private static void WriteResponse(HttpResponse resp, string respString) {
            using StreamWriter writer = new StreamWriter(resp.OutputStream);
            writer.Write(respString);
        }
    }
}
