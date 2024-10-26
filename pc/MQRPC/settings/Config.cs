﻿using Meta_Quest_Discord_Rich_Presence.Properties;
using System.Runtime.Serialization.Formatters.Binary;

namespace MQRPC.settings {

    [Serializable]
    class Config {

        public static DirectoryInfo dir;
        public static FileInfo cfgFile;
        public static Config cfg;

        public bool boot = true;
        public bool sleepWake = true;
        public bool notifs = true;
        public int delay = 3;
        public string address;
        public string apkVersion;


        public static void Init() {
            dir = Directory.CreateDirectory(Environment.GetEnvironmentVariable("APPDATA") + "/MadMagic/" + Resources.name);
            cfgFile = new FileInfo(dir.FullName + "/" + Resources.tag + ".cfg");

            using FileStream fs = GetStream(cfgFile.FullName);
            byte[] data = new byte[fs.Length];
            fs.Read(data, 0, data.Length);

            if (data.Length == 0) {
                cfg = new Config();
                return;
            }

            using MemoryStream ms = new (data);
            cfg = (Config)new BinaryFormatter().Deserialize(ms);
        }

        public static void Save() {
            using MemoryStream ms = new ();
            using FileStream fs = GetStream(cfgFile.FullName);
            new BinaryFormatter().Serialize(ms, cfg);
            byte[] data = ms.ToArray();
            fs.Write(data, 0, data.Length);
        }
        
        public static FileStream GetStream(string dir) {
            return new FileStream(dir, FileMode.OpenOrCreate);
        }
    }
}
