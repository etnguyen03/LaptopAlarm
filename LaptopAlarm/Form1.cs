// This file is part of LaptopAlarm.
// 
// LaptopAlarm is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// LaptopAlarm is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with LaptopAlarm.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AudioSwitcher.AudioApi.CoreAudio;
using System.Diagnostics;
using System.Threading;
using System.IO;
using Microsoft.Win32;
using System.Security.Principal;

namespace LaptopAlarm
{
    public enum onalarm_audio_settings { defaultSound, customSound };
    public partial class Form1 : Form
    {
        private bool allowVisible;
        private bool allowClose;
        private bool arm_letter_entered = false;
        private bool disarm_letter_entered = false;
        private Alarm myAlarm;
        private bool alarmArmed;
        private Form2 alarmForm = new Form2("There is no alarm. Something has gone wrong. Please file a bug report at https://github.com/etnguyen03/LaptopAlarm/issues. Thanks!");
        public CoreAudioDevice playbackDevice = new CoreAudioController().DefaultPlaybackDevice;
        private RegistryKey regKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false);
        private bool initAlarmChecked = false; // variable that stores if the restartalarm check was carried out or not
        private bool initCheckChange = false; // whether or not the settings have been initialized
        private StreamWriter logFileStream;

        // arm keyboard shortcut variables
        Keys arm_key = Keys.A;
        KeyModifiers[] arm_key_modifiers = new KeyModifiers[2] { KeyModifiers.Control, KeyModifiers.Alt };

        // disarm keyboard shortcut variables
        Keys disarm_key = Keys.D;
        KeyModifiers[] disarm_key_modifiers = new KeyModifiers[2] { KeyModifiers.Control, KeyModifiers.Alt };

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //String[] armShortcut = new String[2];
            //armShortcut = Properties.Settings.Default.ArmShortcut.Split(Convert.ToChar(","));
            //foreach (String item in armShortcut)
            //{
            //    textBox1.Text += " " + item + " ";
            //}

            //String[] disarmShortcut = new String[2];
            //disarmShortcut = Properties.Settings.Default.DisarmShortcut.Split(Convert.ToChar(","));
            //foreach (String item in disarmShortcut)
            //{
            //    textBox2.Text += " " + item + " ";
            //}

            // initialize checkbox settings
            checkBox1.Checked = Properties.Settings.Default.trigger_power;
            checkBox2.Checked = Properties.Settings.Default.trigger_usb;
            checkBox3.Checked = Properties.Settings.Default.onalarm_audio;
            if (Properties.Settings.Default.onalarm_audio_default == onalarm_audio_settings.defaultSound)
            {
                radioButton1.Checked = true;
                radioButton2.Checked = false;
            }
            else
            {
                radioButton2.Checked = true;
                radioButton1.Checked = false;
            }
            checkBox4.Checked = Properties.Settings.Default.onalarm_audio_volincrease;
            checkBox5.Checked = Properties.Settings.Default.trigger_battery;
            checkBox6.Checked = Properties.Settings.Default.onalarm_email;
            checkBox8.Checked = Properties.Settings.Default.trigger_restart;
            if (regKey.GetValue("LaptopAlarm") == null)
            {
                checkBox9.Checked = false;
            }
            else
            {
                checkBox9.Checked = true;
            }
            initCheckChange = true;

            // Add UAC shield to checkbox
            // Credits: Serenus, Matthew Ferreira, Mo Patel on Stack Overflow
            if (!IsAdministrator())
            {
                Bitmap shield = SystemIcons.Shield.ToBitmap();
                shield.MakeTransparent();

                pictureBox2.BackgroundImage = shield;
            }            
        }

        protected override void SetVisibleCore(bool value)
        {
            if (allowVisible == false)
            {
                value = false;
                if (!this.IsHandleCreated)
                {
                    CreateHandle();
                }
            }
            if (initAlarmChecked == false)
            {
                ProgramLoad();
                initAlarmChecked = true;
            }
            base.SetVisibleCore(value);
        }

        // Program load function
        private void ProgramLoad()
        {
            // Program load:
            myAlarm = new Alarm(Properties.Settings.Default.onalarm_audio, Properties.Settings.Default.onalarm_audio_default, Properties.Settings.Default.CustomAudioFilePath, Properties.Settings.Default.onalarm_audio_volincrease);
            // ARM keyboard shortcut
            HotKeyManager.RegisterHotKey(Keys.A, KeyModifiers.Control | KeyModifiers.Alt);

            // register the DISARM keyboard shortcut
            HotKeyManager.RegisterHotKey(Keys.D, KeyModifiers.Alt | KeyModifiers.Control);

            HotKeyManager.HotKeyPressed += HotKeyManager_HotKeyPressed;

            workerPowerThread = new Thread(new ThreadStart(monitorPower));
            workerVolThread = new Thread(new ThreadStart(setVolume));

            // Initialize the logfile if not present
            if (!File.Exists(Path.GetDirectoryName(Application.ExecutablePath) + "\\alarmlogs.txt"))
            {
                logFileStream = File.CreateText(Path.GetDirectoryName(Application.ExecutablePath) + "\\alarmlogs.txt");
            }
            else
            {
                logFileStream = new StreamWriter(Path.GetDirectoryName(Application.ExecutablePath) + "\\alarmlogs.txt");
            }
            logFileStream.AutoFlush = true;

            // See if alarmstatus.txt exists
            if (File.Exists(Path.GetDirectoryName(Application.ExecutablePath) + "\\alarmstatus.txt"))
            {
                String alarmdescription = File.ReadAllText(Path.GetDirectoryName(Application.ExecutablePath) + "\\alarmstatus.txt");
                alarmArmed = true;
                toggleToolStripMenuItems();
                myAlarm.causeAlarm();
                stopVolProcess = false;
                workerVolThread = new Thread(new ThreadStart(setVolume));
                workerVolThread.Start();
                notifyIcon2.ShowBalloonTip(1000, "ALARM", alarmdescription + Environment.NewLine + "Realarm at " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString(), ToolTipIcon.Warning);
                alarmForm = new Form2(alarmdescription + Environment.NewLine + "Realarm at " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
                BeginInvoke(new Action(() => { alarmForm.Show(); }));
                workerPowerThread.Abort();
            }
            else
            {
                // Trigger the alarm if alarmarmed.txt exists and the setting is enabled
                if (File.Exists(Path.GetDirectoryName(Application.ExecutablePath) + "\\alarmarmed.txt"))
                {
                    if (Properties.Settings.Default.trigger_restart == true)
                    {
                        String alarmdescription = "ALARM: Restart alarm at " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
                        alarmArmed = true;
                        toggleToolStripMenuItems();
                        myAlarm.causeAlarm();
                        stopVolProcess = false;
                        workerVolThread = new Thread(new ThreadStart(setVolume));
                        workerVolThread.Start();
                        notifyIcon2.ShowBalloonTip(1000, "ALARM", alarmdescription, ToolTipIcon.Warning);
                        alarmForm = new Form2(alarmdescription);
                        BeginInvoke(new Action(() => { alarmForm.Show(); }));
                        workerPowerThread.Abort();
                        logFileStream.WriteLine(alarmdescription);
                        logFileStream.Flush();
                    }
                    else
                    {
                        // arm the alarm
                        // if changing this area, change it in the keyboard shortcut too
                        alarmArmed = true;
                        notifyIcon2.ShowBalloonTip(100, "LaptopAlarm", "Armed", ToolTipIcon.Info);
                        toolStripMenuItem2.Enabled = false;
                        toolStripMenuItem3.Enabled = true;

                        // write to the alarmarmed.txt
                        File.WriteAllText(Path.GetDirectoryName(Application.ExecutablePath) + "\\alarmarmed.txt", "armed " + DateTime.Now.ToShortDateString().ToString() + DateTime.Now.ToShortTimeString().ToString());

                        if (Properties.Settings.Default.trigger_power)
                        {
                            StopPowerProcess = false;
                            workerPowerThread = new Thread(new ThreadStart(monitorPower));
                            workerPowerThread.Start();
                        }
                        if (Properties.Settings.Default.trigger_battery)
                        {
                            StopBatProcess = false;
                            workerBatThread = new Thread(new ThreadStart(monitorBattery));
                            workerBatThread.Start();
                        }
                    }
                }
            }
        }

        // hotkey pressed
        private void HotKeyManager_HotKeyPressed(object sender, HotKeyEventArgs e)
        {
            bool isArm = true;
            foreach (var item in arm_key_modifiers)
            {
                if (item != 0)
                {
                    if (!e.Modifiers.ToString().Contains(item.ToString()))
                    {
                        isArm = false;
                    }
                }
            }
            if (e.Key.ToString() != arm_key.ToString())
            {
                isArm = false;
            }
            if (isArm)
            {
                if (alarmArmed == false)
                {
                    // arm the alarm
                    alarmArmed = true;
                    notifyIcon2.ShowBalloonTip(100, "LaptopAlarm", "Armed", ToolTipIcon.Info);
                    toggleToolStripMenuItems();

                    // write to the alarmarmed.txt
                    File.WriteAllText(Path.GetDirectoryName(Application.ExecutablePath) + "\\alarmarmed.txt", "armed " + DateTime.Now.ToShortDateString().ToString() + DateTime.Now.ToShortTimeString().ToString());

                    if (Properties.Settings.Default.trigger_power)
                    {
                        StopPowerProcess = false;
                        workerPowerThread = new Thread(new ThreadStart(monitorPower));
                        workerPowerThread.Start();
                    }
                    if (Properties.Settings.Default.trigger_battery)
                    {
                        StopBatProcess = false;
                        workerBatThread = new Thread(new ThreadStart(monitorBattery));
                        workerBatThread.Start();
                    }
                }
            }
            else
            {
                // disarm the alarm
                if (alarmArmed == true)
                {
                    alarmArmed = false;
                    myAlarm.stopAlarm();
                    stopVolProcess = true;
                    if (workerVolThread != null)
                    {
                        if (workerVolThread.IsAlive == true)
                        {
                            workerVolThread.Abort();
                        }
                    }
                    if (workerPowerThread != null)
                    {
                        if (workerPowerThread.IsAlive == true)
                        {
                            workerPowerThread.Abort();
                        }
                    }
                    if (workerBatThread != null)
                    {
                        if (workerBatThread.IsAlive == true)
                        {
                            workerBatThread.Abort();
                        }
                    }
                    if (File.Exists(Path.GetDirectoryName(Application.ExecutablePath) + "\\alarmstatus.txt"))
                    {
                        File.Delete(Path.GetDirectoryName(Application.ExecutablePath) + "\\alarmstatus.txt");
                    }
                    if (File.Exists(Path.GetDirectoryName(Application.ExecutablePath) + "\\alarmarmed.txt"))
                    {
                        File.Delete(Path.GetDirectoryName(Application.ExecutablePath) + "\\alarmarmed.txt");
                    }
                    alarmForm.form2_close = true;
                    notifyIcon2.ShowBalloonTip(100, "LaptopAlarm", "Disarmed", ToolTipIcon.Info);

                    toggleToolStripMenuItems();
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (allowClose == false)
            {
                this.Hide();
                e.Cancel = true;
            }
            base.OnFormClosing(e);
        }

        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            allowVisible = true;
            Show();
        }
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            allowClose = true;
            Application.Exit();
        }

        private void contextMenuStrip2_Opening(object sender, CancelEventArgs e)
        {

        }

        //private void textBox1_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Control == true)
        //    {
        //        if (!textBox1.Text.Contains("Control"))
        //        {
        //            textBox1.Text += " Control ";
        //        }
        //    }
        //    else if (e.Alt == true)
        //    {
        //        if (!textBox1.Text.Contains("Alt"))
        //        {
        //            textBox1.Text += " Alt ";
        //        }
        //    }
        //    else if (e.Shift == true)
        //    {
        //        if (!textBox1.Text.Contains("Shift"))
        //        {
        //            textBox1.Text += " Shift ";
        //        }
        //    }
        //    else if (e.KeyCode >= Keys.A && e.KeyCode <= Keys.Z)
        //    {
        //        if (textBox1.Text.Contains("Control") || textBox1.Text.Contains("Alt") || textBox1.Text.Contains("Shift"))
        //        {
        //            if (arm_letter_entered == false)
        //            {
        //                textBox1.Text += "" + e.KeyCode.ToString() + "";
        //                arm_letter_entered = true;
        //            }
        //        }
        //        else if (arm_letter_entered == true)
        //        {
        //            // empty on purpose
        //        }
        //        else
        //        {
        //            toolTip1.Show("You must first press either Control, Alt, or Shift.", textBox1);
        //        }
        //    }
        //}

        //private void button1_Click(object sender, EventArgs e)
        //{
        //    arm_letter_entered = false;
        //    textBox1.Text = "";
        //}

        //private void textBox1_Leave(object sender, EventArgs e)
        //{
        //    String textbox_arm = textBox1.Text.Replace("  ", " ");
        //    textbox_arm = textbox_arm.Trim();

        //    String[] new_arm_kbd = textbox_arm.Split(Char.Parse(" "));
        //    String new_arm_shortcut = "";
        //    foreach (String kbd_item in new_arm_kbd)
        //    {
        //        new_arm_shortcut = kbd_item + ",";
        //    }
        //    new_arm_shortcut = new_arm_shortcut.Remove(new_arm_shortcut.Length - 1);
        //    Properties.Settings.Default.ArmShortcut = new_arm_shortcut;
        //    Properties.Settings.Default.Save();
        //}

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                Properties.Settings.Default.trigger_usb = true;
            }
            else
            {
                Properties.Settings.Default.trigger_usb = false;
            }

            Properties.Settings.Default.Save();
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked == true)
            {
                Properties.Settings.Default.onalarm_audio = true;
                groupBox5.Enabled = true;
            }
            else
            {
                Properties.Settings.Default.onalarm_audio = false;
                groupBox5.Enabled = false;
            }
            myAlarm = new Alarm(Properties.Settings.Default.onalarm_audio, Properties.Settings.Default.onalarm_audio_default, Properties.Settings.Default.CustomAudioFilePath, Properties.Settings.Default.onalarm_audio_volincrease);
            Properties.Settings.Default.Save();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                button2.Enabled = true;
                Properties.Settings.Default.onalarm_audio_default = onalarm_audio_settings.customSound;
                if (Properties.Settings.Default.CustomAudioFilePath == "" || Properties.Settings.Default.CustomAudioFilePath == null)
                {
                    OpenFileDialog file_dialog = new OpenFileDialog();
                    file_dialog.Filter = "Audio files (*.wav)|*.wav";
                    if (file_dialog.ShowDialog() == DialogResult.OK)
                    {
                        Properties.Settings.Default.CustomAudioFilePath = file_dialog.FileName;
                    }
                    else
                    {
                        radioButton1.Checked = true;
                        radioButton2.Checked = false;
                    }
                }
            }
            else
            {
                button2.Enabled = false;
                Properties.Settings.Default.onalarm_audio_default = onalarm_audio_settings.defaultSound;
            }
            myAlarm = new Alarm(Properties.Settings.Default.onalarm_audio, Properties.Settings.Default.onalarm_audio_default, Properties.Settings.Default.CustomAudioFilePath, Properties.Settings.Default.onalarm_audio_volincrease);
            Properties.Settings.Default.Save();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog file_dialog = new OpenFileDialog();
            file_dialog.Filter = "Audio files (*.wav)|*.wav";
            if (file_dialog.ShowDialog() == DialogResult.OK)
            {
                Properties.Settings.Default.CustomAudioFilePath = file_dialog.FileName;
            }
            myAlarm = new Alarm(Properties.Settings.Default.onalarm_audio, Properties.Settings.Default.onalarm_audio_default, Properties.Settings.Default.CustomAudioFilePath, Properties.Settings.Default.onalarm_audio_volincrease);
            Properties.Settings.Default.Save();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                Properties.Settings.Default.trigger_power = true;
            }
            else
            {
                Properties.Settings.Default.trigger_power = false;
            }
            Properties.Settings.Default.Save();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                Properties.Settings.Default.onalarm_audio_default = onalarm_audio_settings.defaultSound;    
            }
            else
            {
                Properties.Settings.Default.onalarm_audio_default = onalarm_audio_settings.customSound;
            }
            myAlarm = new Alarm(Properties.Settings.Default.onalarm_audio, Properties.Settings.Default.onalarm_audio_default, Properties.Settings.Default.CustomAudioFilePath, Properties.Settings.Default.onalarm_audio_volincrease);
            Properties.Settings.Default.Save();
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked)
            {
                Properties.Settings.Default.onalarm_audio_volincrease = true;
                trackBar1.Enabled = true;
            }
            else
            {
                Properties.Settings.Default.onalarm_audio_volincrease = false;
                trackBar1.Enabled = false;
            }
            myAlarm = new Alarm(Properties.Settings.Default.onalarm_audio, Properties.Settings.Default.onalarm_audio_default, Properties.Settings.Default.CustomAudioFilePath, Properties.Settings.Default.onalarm_audio_volincrease);
            Properties.Settings.Default.Save();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (button3.Text == "Test alarm")
            {
                myAlarm.causeAlarm();
                button3.Text = "Stop";
            }
            else
            {
                myAlarm.stopAlarm();
                button3.Text = "Test alarm";
            }
        }

        // Arm tool strip menu
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            // if changing this area, change it in the keyboard shortcut too
            alarmArmed = true;
            notifyIcon2.ShowBalloonTip(100, "LaptopAlarm", "Armed", ToolTipIcon.Info);
            toolStripMenuItem2.Enabled = false;
            toolStripMenuItem3.Enabled = true;

            // write to the alarmarmed.txt
            File.WriteAllText(Path.GetDirectoryName(Application.ExecutablePath) + "\\alarmarmed.txt", "armed " + DateTime.Now.ToShortDateString().ToString() + DateTime.Now.ToShortTimeString().ToString());

            if (Properties.Settings.Default.trigger_power)
            {
                StopPowerProcess = false;
                workerPowerThread = new Thread(new ThreadStart(monitorPower));
                workerPowerThread.Start();
            }
            if (Properties.Settings.Default.trigger_battery)
            {
                StopBatProcess = false;
                workerBatThread = new Thread(new ThreadStart(monitorBattery));
                workerBatThread.Start();
            }
        }

        // Disarm tool strip menu
        public void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            alarmArmed = false;
            myAlarm.stopAlarm();
            stopVolProcess = true;
            if (workerVolThread != null)
            {
                if (workerVolThread.IsAlive == true)
                {
                    workerVolThread.Abort();
                }
            }
            if (workerPowerThread != null)
            {
                if (workerPowerThread.IsAlive == true)
                {
                    workerPowerThread.Abort();
                }
            }
            if (workerBatThread != null)
            {
                if (workerBatThread.IsAlive == true)
                {
                    workerBatThread.Abort();
                }
            }

            if (File.Exists(Path.GetDirectoryName(Application.ExecutablePath) + "\\alarmstatus.txt"))
            {
                File.Delete(Path.GetDirectoryName(Application.ExecutablePath) + "\\alarmstatus.txt");
            }
            if (File.Exists(Path.GetDirectoryName(Application.ExecutablePath) + "\\alarmarmed.txt"))
            {
                File.Delete(Path.GetDirectoryName(Application.ExecutablePath) + "\\alarmarmed.txt");
            }

            alarmForm.form2_close = true;
            notifyIcon2.ShowBalloonTip(100, "LaptopAlarm", "Disarmed", ToolTipIcon.Info);

            toolStripMenuItem2.Enabled = true;
            toolStripMenuItem3.Enabled = false;
        }

        // USB drive detector
        private const int WM_DEVICECHANGE = 0x219;
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            switch (m.Msg)
            {
                case WM_DEVICECHANGE:
                    switch ((int)m.WParam)
                    {
                        case DBT_DEVICEREMOVECOMPLETE:
                            if (alarmArmed && Properties.Settings.Default.trigger_usb)
                            {
                                if (Properties.Settings.Default.onalarm_email)
                                {
                                    sendEmail email = new sendEmail(Properties.Settings.Default.email_to, Properties.Settings.Default.email_smtp_server, Properties.Settings.Default.email_smtp_ssl,Properties.Settings.Default.email_smtp_port,Properties.Settings.Default.email_smtp_auth_username, Properties.Settings.Default.email_smtp_auth_password);
                                    email.sendtheEmail("ALARM: USB Device removed at " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
                                }
                               myAlarm.causeAlarm();
                               stopVolProcess = false;
                               workerVolThread = new Thread(new ThreadStart(setVolume));
                               workerVolThread.Start();
                               notifyIcon2.ShowBalloonTip(1000, "ALARM", "USB Device Removed at " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString(), ToolTipIcon.Warning);
                               alarmForm = new Form2("ALARM: USB Device removed at " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
                               alarmForm.Show();
                               logFileStream.WriteLine("ALARM: USB Device removed at " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
                               logFileStream.Flush();
                            }
                            break;
                    }
                    break;
            }
        }

        private void powerAlarm()
        {
            if (Properties.Settings.Default.onalarm_email)
            {
                sendEmail email = new sendEmail(Properties.Settings.Default.email_to, Properties.Settings.Default.email_smtp_server, Properties.Settings.Default.email_smtp_ssl, Properties.Settings.Default.email_smtp_port, Properties.Settings.Default.email_smtp_auth_username, Properties.Settings.Default.email_smtp_auth_password);
                email.sendtheEmail("ALARM: AC adapter unplugged at " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
            }
            myAlarm.causeAlarm();
            stopVolProcess = false;
            workerVolThread = new Thread(new ThreadStart(setVolume));
            workerVolThread.Start();
            notifyIcon2.ShowBalloonTip(1000, "ALARM", "AC adapter unplugged at " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString(), ToolTipIcon.Warning);
            alarmForm = new Form2("ALARM: AC adapter unplugged at " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
            BeginInvoke(new Action(() => { alarmForm.Show(); }));
            workerPowerThread.Abort();
            logFileStream.WriteLine("ALARM: AC adapter unplugged at " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
            logFileStream.Flush();
        }

        // power detector
        private Thread workerPowerThread = null;
        private bool StopPowerProcess;
        private PowerStatus powerStatus = SystemInformation.PowerStatus;
        private void monitorPower()
        {
            while (StopPowerProcess == false)
            {
                if (powerStatus.PowerLineStatus != PowerLineStatus.Online)
                {
                    if (alarmArmed && Properties.Settings.Default.trigger_power)
                    {
                        Thread.Sleep(1000);
                        if (powerStatus.PowerLineStatus != PowerLineStatus.Online)
                        {
                            powerAlarm();
                        }
                    }
                }
                Thread.Sleep(1000);
            }
            workerPowerThread.Abort();
        }

        // battery removal detector
        private Thread workerBatThread = null;
        private bool StopBatProcess;
        private PowerStatus batteryStatus = SystemInformation.PowerStatus;
        private void monitorBattery()
        {
            while (StopBatProcess == false)
            {
                if (batteryStatus.BatteryChargeStatus == BatteryChargeStatus.NoSystemBattery || batteryStatus.BatteryChargeStatus == BatteryChargeStatus.Unknown)
                {
                    if (alarmArmed && Properties.Settings.Default.trigger_battery)
                    {
                        batteryAlarm();
                        break;
                    }
                }
                Thread.Sleep(1000);
            }
            workerBatThread.Abort();
        }

        private void batteryAlarm()
        {
            if (Properties.Settings.Default.onalarm_email)
            {
                sendEmail email = new sendEmail(Properties.Settings.Default.email_to, Properties.Settings.Default.email_smtp_server, Properties.Settings.Default.email_smtp_ssl, Properties.Settings.Default.email_smtp_port, Properties.Settings.Default.email_smtp_auth_username, Properties.Settings.Default.email_smtp_auth_password);
                email.sendtheEmail("ALARM: Battery removed at " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
            }
            myAlarm.causeAlarm();
            stopVolProcess = false;
            workerVolThread = new Thread(new ThreadStart(setVolume));
            workerVolThread.Start();
            notifyIcon2.ShowBalloonTip(1000, "ALARM", "Battery removed at " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString(), ToolTipIcon.Warning);
            alarmForm = new Form2("ALARM: Battery removed at " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
            BeginInvoke(new Action(() => { alarmForm.Show(); }));
            workerBatThread.Abort();
            logFileStream.WriteLine("ALARM: Battery removed at " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
            logFileStream.Flush();
        }

        // volume controls
        private const int APPCOMMAND_VOLUME_MUTE = 0x80000;
        private const int APPCOMMAND_VOLUME_UP = 0xA0000;
        private const int APPCOMMAND_VOLUME_DOWN = 0x90000;
        private const int WM_APPCOMMAND = 0x319;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg,
            IntPtr wParam, IntPtr lParam);


        private Thread workerVolThread = null;
        private bool stopVolProcess = false;
        private void setVolume()
        {
            while (stopVolProcess == false)
            {
                playbackDevice.Volume = Properties.Settings.Default.onalarm_set_volumeto;
                if (playbackDevice.IsMuted)
                {
                    playbackDevice.ToggleMute();
                }
                Thread.Sleep(500);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        //private void textBox2_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Control == true)
        //    {
        //        if (!textBox2.Text.Contains("Control"))
        //        {
        //            textBox2.Text += " Control ";
        //        }
        //    }
        //    else if (e.Alt == true)
        //    {
        //        if (!textBox2.Text.Contains("Alt"))
        //        {
        //            textBox2.Text += " Alt ";
        //        }
        //    }
        //    else if (e.Shift == true)
        //    {
        //        if (!textBox2.Text.Contains("Shift"))
        //        {
        //            textBox2.Text += " Shift ";
        //        }
        //    }
        //    else if (e.KeyCode >= Keys.A && e.KeyCode <= Keys.Z)
        //    {
        //        if (textBox2.Text.Contains("Control") || textBox2.Text.Contains("Alt") || textBox2.Text.Contains("Shift"))
        //        {
        //            if (disarm_letter_entered == false)
        //            {
        //                textBox2.Text += "" + e.KeyCode.ToString() + "";
        //                disarm_letter_entered = true;
        //            }
        //        }
        //        else if (disarm_letter_entered == true)
        //        {
        //            // empty on purpose
        //        }
        //        else
        //        {
        //            toolTip1.Show("You must first press either Control, Alt, or Shift.", textBox2);
        //        }
        //    }
        //}

        //private void button5_Click(object sender, EventArgs e)
        //{
        //    disarm_letter_entered = false;
        //    textBox2.Text = "";
        //}

        public void toggleToolStripMenuItems()
        {
            //if (toolStripMenuItem2.Enabled == true)
            //{
            //    toolStripMenuItem2.Enabled = false;
            //}
            //else
            //{
            //    toolStripMenuItem2.Enabled = true;
            //}
            //if (toolStripMenuItem3.Enabled == true)
            //{
            //    toolStripMenuItem3.Enabled = false;
            //}
            //else
            //{
            //    toolStripMenuItem3.Enabled = true;
            //}
            backgroundWorker1.RunWorkerAsync();
        }
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (toolStripMenuItem2.Enabled == true)
            {
                if (contextMenuStrip2.InvokeRequired)
                {
                    contextMenuStrip2.Invoke(new MethodInvoker(delegate { toolStripMenuItem2.Enabled = false; }));
                }
                else
                {
                    toolStripMenuItem2.Enabled = false;
                }
            }
            else
            {
                if (contextMenuStrip2.InvokeRequired)
                {
                    contextMenuStrip2.Invoke(new MethodInvoker(delegate { toolStripMenuItem2.Enabled = true; }));
                }
                else
                {
                    toolStripMenuItem2.Enabled = true;
                }
            }
            if (toolStripMenuItem3.Enabled == true)
            {
                if (contextMenuStrip2.InvokeRequired)
                {
                    contextMenuStrip2.Invoke(new MethodInvoker(delegate { toolStripMenuItem3.Enabled = false; }));
                }
                else
                {
                    toolStripMenuItem3.Enabled = false;
                }
            }
            else
            {
                if (contextMenuStrip2.InvokeRequired)
                {
                    contextMenuStrip2.Invoke(new MethodInvoker(delegate { toolStripMenuItem3.Enabled = true; }));
                }
                else
                {
                    toolStripMenuItem3.Enabled = true;
                }
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            Properties.Settings.Default.onalarm_set_volumeto = trackBar1.Value;
            Properties.Settings.Default.Save();
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.trigger_battery = checkBox5.Checked;
            Properties.Settings.Default.Save();
        }

        private void checkBox6_CheckedChanged_1(object sender, EventArgs e)
        {
            if (checkBox6.Checked == true)
            {
                groupBox6.Enabled = true;
                Properties.Settings.Default.onalarm_email = true;
            }
            else
            {
                groupBox6.Enabled = false;
                Properties.Settings.Default.onalarm_email = false;
            }
            Properties.Settings.Default.Save();
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.email_smtp_server = textBox5.Text;
            Properties.Settings.Default.Save();
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.email_to = textBox4.Text;
            Properties.Settings.Default.Save();
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.email_smtp_auth_username = textBox6.Text;
            Properties.Settings.Default.Save();
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.email_smtp_auth_password = textBox7.Text;
            Properties.Settings.Default.Save();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.email_smtp_port = (int)numericUpDown1.Value;
            Properties.Settings.Default.Save();
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox7.Checked)
            {
                Properties.Settings.Default.email_smtp_ssl = true;
            }
            else
            {
                Properties.Settings.Default.email_smtp_ssl = false;
            }
            Properties.Settings.Default.Save();
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox8.Checked)
            {
                Properties.Settings.Default.trigger_restart = true;

                // Check to see if the "start on restart" option is enabled; if not prompt
                if (checkBox9.Checked == false && initCheckChange)
                {
                    if (MessageBox.Show("You've enabled the alarm on restart option; however LaptopAlarm is not configured to start on bootup, and must be for this option to function. Do you want to start LaptopAlarm on bootup?", "LaptopAlarm", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        checkBox9.Checked = true;
                    }
                }
            }
            else
            {
                Properties.Settings.Default.trigger_restart = false;
            }
            Properties.Settings.Default.Save();
        }

        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox9.Checked == true)
            {
                regKey.SetValue("LaptopAlarm", Application.ExecutablePath);
            }
            else
            {
                regKey.DeleteValue("LaptopAlarm");
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // Trigger Alarm
            if (Properties.Settings.Default.onalarm_email)
            {
                sendEmail email = new sendEmail(Properties.Settings.Default.email_to, Properties.Settings.Default.email_smtp_server, Properties.Settings.Default.email_smtp_ssl, Properties.Settings.Default.email_smtp_port, Properties.Settings.Default.email_smtp_auth_username, Properties.Settings.Default.email_smtp_auth_password);
                email.sendtheEmail("ALARM: Manually triggered alarm " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
            }
            myAlarm.causeAlarm();
            alarmArmed = true;
            toggleToolStripMenuItems();
            stopVolProcess = false;
            workerVolThread = new Thread(new ThreadStart(setVolume));
            workerVolThread.Start();
            notifyIcon2.ShowBalloonTip(1000, "ALARM", "Manually triggered alarm at " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString(), ToolTipIcon.Warning);
            alarmForm = new Form2("ALARM: Manually triggered alarm at " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
            BeginInvoke(new Action(() => { alarmForm.Show(); }));
            logFileStream.WriteLine("ALARM: Manually triggered alarm at " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
            logFileStream.Flush();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Check to see if the "alarm logs" tab is selected
            if (tabControl1.SelectedTab == tabPage4)
            {
                logFileStream.Close();
                richTextBox2.Text = File.ReadAllText(Path.GetDirectoryName(Application.ExecutablePath) + "\\alarmlogs.txt");
                logFileStream = new StreamWriter(Path.GetDirectoryName(Application.ExecutablePath) + "\\alarmlogs.txt");
            }
        }

        /// <summary>
        /// Checks whether or not the process is running as an administrator
        /// </summary>
        /// <returns>Boolean: True if running as administrator, false otherwise</returns>
        // Credit: Matthew Ferreira, Mo Patel
        public static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();

            if (identity != null)
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }

            return false;
        }
    }
}
