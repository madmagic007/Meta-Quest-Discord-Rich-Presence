using System.Net;
using System.Net.Sockets;

namespace MQRPC {

    class IPUtils {

        public static bool IsAddress(string address) {
            var parts = address.Split('.');

            return parts.Length == 4
                           && !parts.Any(
                               x => {
                                   return Int32.TryParse(x, out int y) && y > 255 || y < 0;
                               });
        }

        private static string address;
        public static string GetOwnAddress() {
            if (address != null) return address;
            foreach (IPAddress addr in Dns.GetHostAddresses(Dns.GetHostName())) {
                if (addr.AddressFamily == AddressFamily.InterNetwork) {
                    address = addr.ToString();
                    return address;
                }
            }
            return "";
        }
    }
}
