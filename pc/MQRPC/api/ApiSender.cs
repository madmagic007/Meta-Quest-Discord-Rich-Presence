using Flurl.Http;
using Newtonsoft.Json.Linq;
using MQRPC.settings;

namespace MQRPC.api {

    class ApiSender {

        public static JObject Get(string url) => JObject.Parse(url.GetStringAsync().Result);

        public static JObject Post(JObject o, string address = "") {
            if (address == "" && Config.cfg.address != null) address = Config.cfg.address;
            if (address == "") return null;

            o["pcAddress"] = IPUtils.GetOwnAddress();
            o["sleepWake"] = Config.cfg.sleepWake;

            return JObject.Parse(("http://" + address + ":" + ApiSocket.port).PostStringAsync(o.ToString()).ReceiveString().Result);
        }
    }
}
