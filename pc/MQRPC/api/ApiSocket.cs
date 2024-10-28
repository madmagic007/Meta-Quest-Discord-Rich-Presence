using Newtonsoft.Json.Linq;
using NHttp;
using MQRPC.presence;
using MQRPC.settings;
using System.Net;
using System.IO;
using System.Text;

namespace MQRPC.api {

    class ApiSocket : HttpServer {

        public static int port = 16255;

        public ApiSocket() {
            EndPoint = new IPEndPoint(IPAddress.Any, port);
            Console.WriteLine("Started listening at port: " + port);
            Start();
        }

        protected override void OnRequestReceived(HttpRequestEventArgs e) {
            JObject resp = new();
            if (e.Request.InputStream == null) {
                //string base64 = e.Request.QueryString.Get("base64");

                /*if (base64 != null) {
                    byte[] data = Convert.FromBase64String(base64);

                    e.Response.ContentType = "image/jpg";
                    e.Response.OutputStream.Write(data);
                    e.Response.OutputStream.Close();
                } else {*/
                    WriteResponse(e.Response, "Yep, your pc is running");
                //}

                return;
            }
            using StreamReader sr = new(e.Request.InputStream);
            string msg = sr.ReadToEnd();
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
                    if (!DiscordHandler.Init()) return;
                    Program.SendNotif("Starting showing quest presence in discord");
                    Timers.StartRequesting();
                    break;

                case "offline":
                    PresenceHandler.Stop();
                    break;

                case "connect":
                    resp["connected"] = "Successfully connected";
                    break;
            }

            WriteResponse(e.Response, resp.ToString());
        }

        private static void WriteResponse(HttpResponse resp, string respString) {
            using StreamWriter writer = new (resp.OutputStream);
            writer.Write(respString);
        }
    }
}
