﻿using Flurl.Http;
using Newtonsoft.Json.Linq;
using Meta_Quest_Discord_Rich_Presence.Properties;
using MQRPC.settings;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Windows.Forms;

namespace MQRPC.updating {

    class UpdaterGUI : Form {

        private JObject o;

        private Button btnSelf;
        private Button btnApk;

        private Label lblSelf;
        private Label lblApk;

        public UpdaterGUI(bool apk, bool self, JObject o) {
            this.o = o;
            ClientSize = new Size(230, 70);
            Text = Resources.tag + " update";
            Icon = Resources.AppIcon;
            MinimizeBox = false;
            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.FixedSingle;

            lblSelf = new() {
                Text = self ? "New version found" : "No new version found",
                AutoSize = true,
                Location = new Point(3, 7),
            };
            Controls.Add(lblSelf);

            btnSelf = new() {
                Text = "Download",
                Enabled = self,
                Height = 22,
                AutoSize = true
            };
            btnSelf.Location = new Point(ClientSize.Width - btnSelf.Width - 5, 3);
            Controls.Add(btnSelf);
            btnSelf.Click += BtnSelf_Click;

            lblApk = new() {
                Text = apk ? "New Apk version found" : "No new apk version found",
                Location = new Point(3, GetEndY(btnSelf) + 10),
                AutoSize = true
            };
            Controls.Add(lblApk);

            btnApk = new() {
                Text = "Download",
                Enabled = apk,
                Height = 22,
                AutoSize = true
            };
            btnApk.Click += BtnApk_Click;
            btnApk.Location = new Point(ClientSize.Width - btnApk.Width - 5, lblApk.Location.Y - 4);
            Controls.Add(btnApk);
        }

        private void BtnApk_Click(object? sender, EventArgs e) {
            btnApk.Enabled = false;
            lblApk.Text = "Downloading...";

            string url = (string)o["apkUrl"];
            string dir = Config.dir + "\\MQRPC.apk";
            DownloadTo(dir, url).DownloadFileCompleted += (_, _) => {
                lblApk.Text = "Finished download";

                DialogResult d = MessageBox.Show("Updated apk ready to install. Make sure quest is connected via usb", "MQRPC apk update ready to install", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

                if (d == DialogResult.OK) {
                    ADBUtils au = new();
                    if (au.TryGetAddress().Equals("")) {
                        MessageBox.Show("Quest not connected, make sure quest is connected to proceed", "Quest not connected", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        lblApk.Text = "Failed to install";
                        return;
                    }

                    au.Install(dir);
                    Program.SendNotif("MQRPC successfully updated on quest");
                    au.Launch();
                    au.Stop();
                    lblApk.Text = "Sucesfully updated";
                }
            };
        }

        private async void BtnSelf_Click(object? sender, EventArgs e) {
            btnSelf.Enabled = false;
            lblSelf.Text = "Downloading...";
            string url = (string)o["installer"];
            string dir = Config.dir + "\\MQRPC.msi";
            DownloadTo(dir, url).DownloadFileCompleted += (_, _) => {
                lblSelf.Text = "Starting update...";

                FileInfo f = new (dir);

                var p = new Process {
                    StartInfo = new ProcessStartInfo(f.FullName) {
                        UseShellExecute = true
                    }
                };
                p.Start();

                Program.trayIcon.Visible = false;
                Environment.Exit(0);
            };
        }

        private static WebClient DownloadTo(string dir, string url) {
            using WebClient wc = new();
            wc.DownloadFileAsync(new Uri(url), dir);
            return wc;
        }

        private int GetEndY(Control c) {
            return c.Location.Y + c.Height;
        }
    }
}
