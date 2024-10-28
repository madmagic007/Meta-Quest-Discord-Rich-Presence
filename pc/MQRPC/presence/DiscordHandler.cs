using DiscordRPC;
using Newtonsoft.Json.Linq;
namespace MQRPC.presence {

    class DiscordHandler {

        private static DiscordRpcClient client;
        private static string lastId = "";

        public static bool Init(string id = "664525664946356230") {
            if (!lastId.Equals(id)) StopPresence();
            else if (client.IsInitialized) return false; ;

            client = new DiscordRpcClient(id);
            lastId = id;
            client.Initialize();

            client.SetPresence(new RichPresence {
                Details = "Just started playing",
                Assets = new Assets {
                    LargeImageKey = "quest",
                    LargeImageText = "MQRPC by MadMagic"
                }
            });

            return true;
        }

        private static ulong current = 0;
        public static void Handle(JObject o) {
            string curId = (string)o["appId"];
            if (curId != null && !lastId.Equals(curId)) Init(curId);

            Timestamps ts = new();
            if (o.ContainsKey("remaining")) ts.EndUnixMilliseconds = (ulong)DateTimeOffset.Now.ToUnixTimeMilliseconds() + (ulong)o["remaining"];
            if (o.ContainsKey("elapsed")) {
                if ((bool)o["elasped"]) {
                    if (current == 0) current = (ulong) DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    ts.StartUnixMilliseconds = current;
                } else current = 0;
            } else if (o.ContainsKey("startTime")) {
                ts.StartUnixMilliseconds = (ulong)o["startTime"];
            } else current = 0;

            client.SetPresence(new RichPresence {
                Details = (string)o["details"],
                State = (string)o["state"],
                Assets = new Assets {
                    LargeImageKey = (string)o["largeImageKey"],
                    LargeImageText = (string)o["largeImageText"],
                    SmallImageKey = (string)o["smallImageKey"],
                    SmallImageText = (string)o["smallImageText"]
                },
                Timestamps = ts
            });
        }

        public static void StopPresence() {
            if (client == null || client.IsDisposed) return;
            client.Dispose();
        }
    }
}
