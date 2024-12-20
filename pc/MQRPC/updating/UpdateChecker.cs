﻿using Newtonsoft.Json.Linq;
using Flurl.Http;
using MQRPC.settings;

namespace MQRPC.updating {

    class UpdateChecker {

        private static string updateUrl = "https://raw.githubusercontent.com/madmagic007/Meta-Quest-Discord-Rich-Presence/refs/heads/master/update.json";
        private static string version = "3.0.0";

        public static void Check(bool force) {
            JObject o = JObject.Parse(updateUrl.GetStringAsync().Result);

            bool app = !((string)o["latest"]).Equals(version) || force;
            bool apk = Config.cfg.apkVersion != null && !Config.cfg.apkVersion.Equals((string)o["apkVersion"]) || force;

            if (apk || app) new UpdaterGUI(apk, app, o).Show();
        }
    }
}
