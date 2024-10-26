using Newtonsoft.Json.Linq;
using Meta_Quest_Discord_Rich_Presence.Properties;
using MQRPC.api;
using System.Drawing;
using System.Windows.Forms;

namespace MQRPC.settings {

    class SettingsGui : Form {

        private TextBox tbAddress;
        private Label txtFeedback;
        private CheckBox cbBoot;
        private CheckBox cbSleepWake;
        private CheckBox cbNotifs;
        private NumericUpDown nudDelay;
        private Button btnTryGetAddress;

        private bool validated;
        private Config c;
        private ADBUtils au;

        public SettingsGui() {
            c = Config.cfg;

            string address = c.address;
            bool already = address != null;

            ClientSize = new Size(300, 188);
            Text = Resources.name + " settings";
            Icon = Resources.AppIcon;
            MinimizeBox = false;
            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.FixedSingle;

            Label txtAddress = new() {
                Text = "Quest address:",
                Location = new Point(3, 6),
                AutoSize = true
            };
            Controls.Add(txtAddress);

            tbAddress = new TextBox {
                Text = address ?? "Not Set",
                Location = new Point(GetEndX(txtAddress) + 5, txtAddress.Location.Y - 3),
                Width = ClientSize.Width - 10 - GetEndX(txtAddress),
            };
            Controls.Add(tbAddress);

            btnTryGetAddress = new() {
                Text = "Try to get address automatically",
                Width = 190,
                Height = 22,
                Location = new Point(3, GetEndY(tbAddress) + 3),
                Enabled = false
            };
            btnTryGetAddress.Click += (_, _) => {
                txtFeedback.Text = "Attempting to get address automatically...";
                string adbAddress = au.TryGetAddress();

                if (adbAddress != null) {
                    txtFeedback.Text = "Address automatically retrieved";
                    tbAddress.Text = adbAddress.Trim();
                } else txtFeedback.Text = "Failed to retrieve address automatically";
            };
            Controls.Add(btnTryGetAddress);

            Button btnValidate = new() {
                Text = "Validate",
                Height = 22,
            };
            btnValidate.Location = new Point(ClientSize.Width - btnValidate.Width - 5, GetEndY(tbAddress) + 3);
            btnValidate.Click += BtnValidate_Click;
            Controls.Add(btnValidate);

            txtFeedback = new Label {
                Text = already ? "Address read from config" : "Press above button to retrieve quest address",
                Location = new Point(3, GetEndY(btnTryGetAddress) + 3),
                Width = ClientSize.Width - 6
            };
            Controls.Add(txtFeedback);

            Label txtDelay = new() {
                Text = "Presence update delay (Seconds)",
                Location = new Point(3, GetEndY(txtFeedback) + 10),
            };
            Controls.Add(txtDelay);

            nudDelay = new NumericUpDown {
                Value = 3,
                Minimum = 1,
                Location = new Point(btnValidate.Location.X, txtDelay.Location.Y),
                Width = btnValidate.Width - 2,
            };
            Controls.Add(nudDelay);
            txtDelay.Width = nudDelay.Location.X - 10;

            cbBoot = new CheckBox {
                Text = "Start with windows",
                Checked = c.boot,
                Location = new Point(5, GetEndY(txtDelay) + 5),
                AutoSize = true,
            };
            Controls.Add(cbBoot);

            cbSleepWake = new CheckBox {
                Text = "Pause presence when quest screen turns off",
                Checked = c.sleepWake,
                Location = new Point(5, GetEndY(cbBoot) + 5),
                AutoSize = true
            };
            Controls.Add(cbSleepWake);

            cbNotifs = new CheckBox {
                Text = "Notify when presence starts/stops",
                Checked = c.notifs,
                Location = new Point(5, GetEndY(cbSleepWake) + 5),
                AutoSize = true
            };
            Controls.Add(cbNotifs);

            Button btnSave = new() {
                Text = "Save",
                Size = btnValidate.Size,
                Location = new Point(ClientSize.Width - btnValidate.Width - 5, ClientSize.Height - btnValidate.Height - 5),
            };
            Controls.Add(btnSave);
            btnSave.Click += (_, e) => {
                if (validated) c.address = tbAddress.Text;
                c.boot = cbBoot.Checked;
                c.sleepWake = cbSleepWake.Checked;
                c.notifs = cbNotifs.Checked;
                c.delay = (int)nudDelay.Value;
                Config.Save();
                Close();
            };

            FormClosed += (_, _) => {
                au?.Stop();
            };

            Show();

            Task.Run(() => {
                au = new();

                if (!au.WaitforAuth()) return;
                btnTryGetAddress.Enabled = true;

                if (au.IsInstalled()) return;

                DialogResult d = MessageBox.Show("MQRPC app was not detected on your quest, install now?", "MQRPC not detected on quest", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

                if (d == DialogResult.OK) Invoke(() => {
                    au.Install();
                    Program.SendNotif("OQPRC successfully installed on quest");
                    au.Launch();
                });
            });
        }

        private void BtnValidate_Click(object sender, EventArgs e) {
            string address = tbAddress.Text.Trim();
            validated = false;

            Task.Run(() => {
                if (!IPUtils.IsAddress(address) || string.IsNullOrEmpty(address)) {
                    txtFeedback.Text = "Not a valid address";
                    return;
                }

                try {
                    JObject o = ApiSender.Post(new JObject {
                        ["message"] = "validate"
                    }, address);

                    Console.WriteLine(o.ToString());
                    if (!o.ContainsKey("valid")) throw new Exception();

                    txtFeedback.Text = "Successfully validated quest";
                    validated = true;
                } catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                    txtFeedback.Text = "No response received from quest";
                }
            });
        }

        private static int GetEndX(Control c) {
            return c.Location.X + c.Width;
        }

        private static int GetEndY(Control c) {
            return c.Location.Y + c.Height;
        }

        public static T GetIfOpen<T>(bool show = false) where T : Form, new() {
            foreach (Form f in Application.OpenForms) if (f is T t) return t;
            if (show) new T().Show();
            return (T)(object)null;
        }
    }
}