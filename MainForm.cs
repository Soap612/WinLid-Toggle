using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;

namespace LidController
{
    public class MainForm : Form
    {
        private RadioButton rbStayAwake;
        private RadioButton rbDefault;
        private Label lblStatus;
        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;

        private const string REG_KEY = @"Software\LidBehaviorController";
        
        public MainForm()
        {
            InitializeComponent();
            ApplySavedSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "Lid Behavior Controller";
            this.Size = new Size(380, 200);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Icon = SystemIcons.Shield; // Simple default icon

            Label lblDesc = new Label();
            lblDesc.Text = "Select your preferred laptop lid close behavior:";
            lblDesc.Location = new Point(20, 20);
            lblDesc.Size = new Size(320, 20);
            this.Controls.Add(lblDesc);

            rbStayAwake = new RadioButton();
            rbStayAwake.Text = "Keep PC awake when lid is closed";
            rbStayAwake.Location = new Point(30, 50);
            rbStayAwake.Size = new Size(300, 25);
            rbStayAwake.CheckedChanged += RbMode_CheckedChanged;
            this.Controls.Add(rbStayAwake);

            rbDefault = new RadioButton();
            rbDefault.Text = "Use default Windows behavior";
            rbDefault.Location = new Point(30, 80);
            rbDefault.Size = new Size(300, 25);
            rbDefault.CheckedChanged += RbMode_CheckedChanged;
            this.Controls.Add(rbDefault);

            lblStatus = new Label();
            lblStatus.Text = "Current mode: Unknown";
            lblStatus.Location = new Point(20, 120);
            lblStatus.Size = new Size(320, 20);
            lblStatus.Font = new Font(this.Font, FontStyle.Bold);
            this.Controls.Add(lblStatus);

            // Tray Icon Setup
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Show", OnTrayShow);
            trayMenu.MenuItems.Add("-");
            trayMenu.MenuItems.Add("Exit", OnTrayExit);

            trayIcon = new NotifyIcon();
            trayIcon.Text = "Lid Behavior Controller";
            trayIcon.Icon = SystemIcons.Information; // Use another basic icon
            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;
            trayIcon.DoubleClick += OnTrayShow;

            this.FormClosing += MainForm_FormClosing;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
                trayIcon.ShowBalloonTip(2000, "Lid Controller", "Running in the background.", ToolTipIcon.Info);
            }
        }

        private void OnTrayShow(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
        }

        private void OnTrayExit(object sender, EventArgs e)
        {
            trayIcon.Visible = false;
            Application.ExitThread();
            Environment.Exit(0);
        }

        private void RbMode_CheckedChanged(object sender, EventArgs e)
        {
            if (!((RadioButton)sender).Checked) return;

            if (rbStayAwake.Checked)
            {
                EnableAwakeMode();
            }
            else
            {
                RestoreDefaultMode();
            }
        }

        private void EnableAwakeMode()
        {
            Guid? scheme = PowerInterop.GetActiveScheme();
            if (scheme.HasValue)
            {
                // Backup current values before editing, if we aren't already overriding them
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(REG_KEY))
                {
                    bool isAlreadyAwake = ((int)key.GetValue("IsAwakeMode", 0)) == 1;

                    if (!isAlreadyAwake)
                    {
                        uint acValue, dcValue;
                        if (PowerInterop.ReadLidCloseAction(scheme.Value, out acValue, out dcValue))
                        {
                            key.SetValue("SavedSchemeGuid", scheme.Value.ToString());
                            key.SetValue("SavedAcValue", (int)acValue);
                            key.SetValue("SavedDcValue", (int)dcValue);
                        }
                    }

                    // 0 = Do nothing
                    bool success = PowerInterop.WriteLidCloseAction(scheme.Value, 0, 0);
                    if (success)
                    {
                        key.SetValue("IsAwakeMode", 1);
                        UpdateStatus("Stay Awake");
                    }
                    else
                    {
                        UpdateStatus("Error applying Stay Awake");
                    }
                }
            }
        }

        private void RestoreDefaultMode()
        {
            Guid? scheme = PowerInterop.GetActiveScheme();
            if (scheme.HasValue)
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(REG_KEY))
                {
                    string savedGuidStr = key.GetValue("SavedSchemeGuid") as string;
                    int defaultAc = 1; // 1 = Sleep
                    int defaultDc = 1;

                    if (!string.IsNullOrEmpty(savedGuidStr))
                    {
                        Guid savedGuid;
                        if (Guid.TryParse(savedGuidStr, out savedGuid) && savedGuid == scheme.Value)
                        {
                            defaultAc = (int)key.GetValue("SavedAcValue", 1);
                            defaultDc = (int)key.GetValue("SavedDcValue", 1);
                            
                            // Prevent writing 0 if default was 0 initially somehow or corrupted
                            if (defaultAc == 0 && defaultDc == 0)
                            {
                                defaultAc = 1; 
                                defaultDc = 1;
                            }
                        }
                    }

                    bool success = PowerInterop.WriteLidCloseAction(scheme.Value, (uint)defaultAc, (uint)defaultDc);
                    if (success)
                    {
                        key.SetValue("IsAwakeMode", 0);
                        UpdateStatus("Default Windows Behavior");
                    }
                    else
                    {
                        UpdateStatus("Error restoring default");
                    }
                }
            }
        }

        private void UpdateStatus(string mode)
        {
            lblStatus.Text = "Current mode: " + mode;
        }

        private void ApplySavedSettings()
        {
            // Detach events to prevent triggering while setting UI state
            rbStayAwake.CheckedChanged -= RbMode_CheckedChanged;
            rbDefault.CheckedChanged -= RbMode_CheckedChanged;

            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(REG_KEY))
            {
                int isAwake = (int)key.GetValue("IsAwakeMode", 0);
                if (isAwake == 1)
                {
                    rbStayAwake.Checked = true;
                    EnableAwakeMode(); // Reapply to ensure settings stick if OS changed them
                }
                else
                {
                    rbDefault.Checked = true;
                    // Don't auto-restore default on boot just update UI
                    UpdateStatus("Default Windows Behavior");
                }
            }

            // Reattach
            rbStayAwake.CheckedChanged += RbMode_CheckedChanged;
            rbDefault.CheckedChanged += RbMode_CheckedChanged;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            // To start minimized via a command line arg
            string[] args = Environment.GetCommandLineArgs();
            bool startMinimized = false;
            foreach(string arg in args)
            {
                if(arg.ToLower() == "-minimized") 
                    startMinimized = true;
            }

            if(startMinimized)
            {
                this.WindowState = FormWindowState.Minimized;
                this.Hide();
                this.ShowInTaskbar = false;
            }
        }
    }
}
