using MQRPC.settings;
using SharpAdbClient;
using SharpAdbClient.DeviceCommands;
using System.Windows.Forms;

namespace MQRPC {

    class ADBUtils {

        private static string dir = Config.dir.FullName + "/adb/adb.exe";
        private AdbServer server;
        private AdbClient client;
        private DeviceData device;

        public ADBUtils() {
            if (server != null) return;
            server = new AdbServer();
            server.StartServer(dir, false);
            client = new AdbClient();
        }

        public string TryGetAddress() {
            foreach (var device in client.GetDevices()) {
                this.device = device;
                ConsoleOutputReceiver rec = new ();
                client.ExecuteRemoteCommand("ip addr show wlan0 | grep 'inet' | awk '{print $2}' | awk -F'/' '{print $1}'", device, rec);
                string[] split = rec.ToString().Split('\n');
                if (split.Length == 0) continue;
                return split[0];
            }
            return null;
        }

        public bool IsInstalled() {
            if (device == null) return true;
            Console.WriteLine(device == null);
            PackageManager pm = new (client, device);
            return pm.Packages.ContainsKey("com.madmagic.mqrpc");
        }

        public void Install(string dir = "") {
            PackageManager pm = new (client, device);

            if (dir.Equals("")) {
                OpenFileDialog ofd = new () {
                    Title = "Select MQRPC apk",
                    InitialDirectory = ".",
                    Filter = "APK files (*.apk)|*.apk",
                    Multiselect = false
                };
                if (ofd.ShowDialog().Equals(false)) return;
                dir = ofd.FileName;
            }

            pm.InstallPackage(dir, reinstall: false);
        }

        public void Launch() {
            client.ExecuteRemoteCommand("monkey -p com.madmagic.mqrpc 1", device, null);
        }

        public void Stop() {
            try {
                client.KillAdb();
            } catch (Exception) { } //adb already stopped
        }
    }
}
