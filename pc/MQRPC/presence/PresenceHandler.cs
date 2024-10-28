using Flurl.Http;
using Newtonsoft.Json.Linq;

namespace MQRPC.presence {
    class PresenceHandler {

        private static JObject gitObj;
        private static string lastPackage = "";
        private static ulong lastStartTime = 0;

        static PresenceHandler() {
            gitObj = JObject.Parse("https://raw.githubusercontent.com/madmagic007/Meta-Quest-Discord-Rich-Presence/refs/heads/master/lang.json".GetStringAsync().Result);
        }


        public static void Handle(JObject o) {
            Parser p = new(o);

            if (!o.ContainsKey("appId")) {
                o["details"] = p.Get("details", "Currently playing:");
                if (!p.hasGitDetails) o["state"] = p.Get("state", p.name);
                o["largeImageKey"] = p.Get("largeImageKey", "");
                o["largeImageText"] = p.Get("largeImageText", "");
                o["smallImageKey"] = p.Get("smallImageKey", "");
                o["smallImageText"] = p.Get("smallImageText", "");
            }

            if (p.IsEmpty("largeImageKey")) {
                o["largeImageKey"] = "quest";
                o["largeImageText"] = "MQRPC by MadMagic";
            } else if (p.IsEmpty("smallImageKey")) {
                o["smallImageKey"] = "quest";
                o["smallImageText"] = "MQRPC by MadMagic";
            }

            if (!(bool)o["detailed"]) {
                string packageName = (string)o["packageName"];

                if (lastPackage != packageName) {
                    lastPackage = packageName;
                    lastStartTime = (ulong)DateTimeOffset.Now.ToUnixTimeMilliseconds();
                }

                o["startTime"] = lastStartTime;
            }

            DiscordHandler.Handle(o);
        }

        public static void Stop() {
            Timers.StopAll();
            Program.SendNotif("Stopped showing quest presence in discord");
            DiscordHandler.StopPresence();
        }

        internal class Parser {

            internal string packageName;
            internal string name;
            JObject o;
            internal bool hasGitDetails;


            public Parser(JObject o) {
                this.o = o;
                packageName = (string)o["packageName"];
                name = (string)o["name"];
            }

            public string Get(string tag, string fallback) {
                if (!gitObj.ContainsKey(packageName)) return fallback;
                JObject gameObj = (JObject)gitObj[packageName];
                hasGitDetails = tag == "details" && gameObj.ContainsKey(tag);
                return (string)gameObj[tag] ?? fallback;
            }

            public bool IsEmpty(string tag) {
                return !o.ContainsKey(tag) || ((string)o[tag]).Equals("");
            }
        }
    }
}
