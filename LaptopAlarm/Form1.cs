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

        // arm keyboard shortcut variables
        Keys arm_key = new Keys();
        KeyModifiers[] arm_key_modifiers = new KeyModifiers[2];

        // disarm keyboard shortcut variables
        Keys disarm_key = new Keys();
        KeyModifiers[] disarm_key_modifiers = new KeyModifiers[2];

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            String[] armShortcut = new String[2];
            armShortcut = Properties.Settings.Default.ArmShortcut.Split(Convert.ToChar(","));
            foreach (String item in armShortcut)
            {
                textBox1.Text += " " + item + " ";
            }

            String[] disarmShortcut = new String[2];
            disarmShortcut = Properties.Settings.Default.DisarmShortcut.Split(Convert.ToChar(","));
            foreach (String item in disarmShortcut)
            {
                textBox2.Text += " " + item + " ";
            }

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
        }

        protected override void SetVisibleCore(bool value)
        {
            // Program load:
            myAlarm = new Alarm(Properties.Settings.Default.onalarm_audio, Properties.Settings.Default.onalarm_audio_default, Properties.Settings.Default.CustomAudioFilePath, Properties.Settings.Default.onalarm_audio_volincrease);
            // ARM keyboard shortcut
            String[] armShortcut = new String[2];
            armShortcut = Properties.Settings.Default.ArmShortcut.Split(Convert.ToChar(","));
            Keys key = new Keys();
            KeyModifiers[] keyModifierList = new KeyModifiers[3];
            int i = 0;
            foreach (String item in armShortcut)
            {
                if (item.Length == 1)
                {
                    key = (Keys)Enum.Parse(typeof(Keys), item);
                }
                else
                {
                    keyModifierList[i] = (KeyModifiers)Enum.Parse(typeof(KeyModifiers), item);
                    i++;
                }
            }

            arm_key = key;
            arm_key_modifiers = keyModifierList;

            // register keyboard shortcut

            if (keyModifierList.Contains((KeyModifiers)Enum.Parse(typeof(KeyModifiers), "Control")) && (keyModifierList.Contains((KeyModifiers)Enum.Parse(typeof(KeyModifiers), "Shift")) && (keyModifierList.Contains((KeyModifiers)Enum.Parse(typeof(KeyModifiers), "Alt")))))
            {
                HotKeyManager.RegisterHotKey(key, KeyModifiers.Control | KeyModifiers.Shift | KeyModifiers.Alt);
            }
            else if (keyModifierList.Contains((KeyModifiers)Enum.Parse(typeof(KeyModifiers), "Control")) && (keyModifierList.Contains((KeyModifiers)Enum.Parse(typeof(KeyModifiers), "Alt")) && keyModifierList.Length == 3))
            {
                HotKeyManager.RegisterHotKey(key, KeyModifiers.Control | KeyModifiers.Alt);
            }
            else if (keyModifierList.Contains((KeyModifiers)Enum.Parse(typeof(KeyModifiers), "Control")) && (keyModifierList.Contains((KeyModifiers)Enum.Parse(typeof(KeyModifiers), "Shift")) && keyModifierList.Length == 3))
            {
                HotKeyManager.RegisterHotKey(key, KeyModifiers.Control | KeyModifiers.Shift);
            }
            else if (keyModifierList.Contains((KeyModifiers)Enum.Parse(typeof(KeyModifiers), "Alt")) && (keyModifierList.Contains((KeyModifiers)Enum.Parse(typeof(KeyModifiers), "Shift")) && keyModifierList.Length == 3))
            {
                HotKeyManager.RegisterHotKey(key, KeyModifiers.Alt | KeyModifiers.Shift);
            }
            else if (keyModifierList.Contains((KeyModifiers)Enum.Parse(typeof(KeyModifiers), "Control")) && keyModifierList.Length == 2)
            {
                HotKeyManager.RegisterHotKey(key, KeyModifiers.Control);
            }
            else if (keyModifierList.Contains((KeyModifiers)Enum.Parse(typeof(KeyModifiers), "Alt")) && keyModifierList.Length == 2)
            {
                HotKeyManager.RegisterHotKey(key, KeyModifiers.Alt);
            }
            else if (keyModifierList.Contains((KeyModifiers)Enum.Parse(typeof(KeyModifiers), "Shift")) && keyModifierList.Length == 2)
            {
                HotKeyManager.RegisterHotKey(key, KeyModifiers.Shift);
            }
            else
            {
                MessageBox.Show("An error has occured. Please file a bug report at https://github.com/etnguyen03/LaptopAlarm/issues. Error code ARM_KBD_ELSE");
                throw new InvalidExpressionException();
            }

            // register the DISARM keyboard shortcut
            String[] disarmShortcut = new String[2];
            disarmShortcut = Properties.Settings.Default.DisarmShortcut.Split(Convert.ToChar(","));
            Keys disarmKey = new Keys();
            KeyModifiers[] disarmKeyModifierList = new KeyModifiers[3];
            int disi = 0;
            foreach (String item in disarmShortcut)
            {
                if (item.Length == 1)
                {
                    disarmKey = (Keys)Enum.Parse(typeof(Keys), item);
                }
                else
                {
                    disarmKeyModifierList[disi] = (KeyModifiers)Enum.Parse(typeof(KeyModifiers), item);
                    disi++;
                }
            }

            disarm_key = disarmKey;
            disarm_key_modifiers = disarmKeyModifierList;

            // register keyboard shortcut

            if (disarmKeyModifierList.Contains((KeyModifiers)Enum.Parse(typeof(KeyModifiers), "Control")) && (disarmKeyModifierList.Contains((KeyModifiers)Enum.Parse(typeof(KeyModifiers), "Shift")) && (disarmKeyModifierList.Contains((KeyModifiers)Enum.Parse(typeof(KeyModifiers), "Alt")))))
            {
                HotKeyManager.RegisterHotKey(disarmKey, KeyModifiers.Control | KeyModifiers.Shift | KeyModifiers.Alt);
            }
            else if (disarmKeyModifierList.Contains((KeyModifiers)Enum.Parse(typeof(KeyModifiers), "Control")) && (disarmKeyModifierList.Contains((KeyModifiers)Enum.Parse(typeof(KeyModifiers), "Shift")) && disarmKeyModifierList.Length == 3))
            {
                HotKeyManager.RegisterHotKey(disarmKey, KeyModifiers.Control | KeyModifiers.Shift);
            }
            else if (disarmKeyModifierList.Contains((KeyModifiers)Enum.Parse(typeof(KeyModifiers), "Alt")) && (disarmKeyModifierList.Contains((KeyModifiers)Enum.Parse(typeof(KeyModifiers), "Shift")) && disarmKeyModifierList.Length == 3))
            {
                HotKeyManager.RegisterHotKey(disarmKey, KeyModifiers.Alt | KeyModifiers.Shift);
            }
            else if (disarmKeyModifierList.Contains((KeyModifiers)Enum.Parse(typeof(KeyModifiers), "Control")) && (disarmKeyModifierList.Contains((KeyModifiers)Enum.Parse(typeof(KeyModifiers), "Alt")) && disarmKeyModifierList.Length == 3))
            {
                HotKeyManager.RegisterHotKey(disarmKey, KeyModifiers.Control | KeyModifiers.Alt);
            }
            else if (disarmKeyModifierList.Contains((KeyModifiers)Enum.Parse(typeof(KeyModifiers), "Control")) && disarmKeyModifierList.Length == 2)
            {
                HotKeyManager.RegisterHotKey(disarmKey, KeyModifiers.Control);
            }
            else if (disarmKeyModifierList.Contains((KeyModifiers)Enum.Parse(typeof(KeyModifiers), "Alt")) && disarmKeyModifierList.Length == 2)
            {
                HotKeyManager.RegisterHotKey(disarmKey, KeyModifiers.Alt);
            }
            else if (disarmKeyModifierList.Contains((KeyModifiers)Enum.Parse(typeof(KeyModifiers), "Shift")) && disarmKeyModifierList.Length == 2)
            {
                HotKeyManager.RegisterHotKey(disarmKey, KeyModifiers.Shift);
            }
            else
            {
                MessageBox.Show("An error has occured. Please file a bug report at https://github.com/etnguyen03/LaptopAlarm/issues. Error code DISARM_KBD_ELSE");
                throw new InvalidExpressionException();
            }

            HotKeyManager.HotKeyPressed += HotKeyManager_HotKeyPressed;

            if (allowVisible == false)
            {
                value = false;
                if (!this.IsHandleCreated)
                {
                    CreateHandle();
                }
            }

            workerBatThread = new Thread(new ThreadStart(monitorBattery));
            workerVolThread = new Thread(new ThreadStart(setVolume));

            base.SetVisibleCore(value);
        }

        // hotkey pressed
        private void HotKeyManager_HotKeyPressed(object sender, HotKeyEventArgs e)
        {
            MessageBox.Show(e.Modifiers.ToString());
            MessageBox.Show(e.Key.ToString());
            // arm the alarm
            alarmArmed = true;
            notifyIcon2.ShowBalloonTip(100, "LaptopAlarm", "Armed", ToolTipIcon.Info);
            backgroundWorker1.RunWorkerAsync();

            if (Properties.Settings.Default.trigger_power)
            {
                stopBatProcess = false;
                workerBatThread = new Thread(new ThreadStart(monitorBattery));
                workerBatThread.Start();
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

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control == true)
            {
                if (!textBox1.Text.Contains("Control"))
                {
                    textBox1.Text += " Control ";
                }
            }
            else if (e.Alt == true)
            {
                if (!textBox1.Text.Contains("Alt"))
                {
                    textBox1.Text += " Alt ";
                }
            }
            else if (e.Shift == true)
            {
                if (!textBox1.Text.Contains("Shift"))
                {
                    textBox1.Text += " Shift ";
                }
            }
            else if (e.KeyCode >= Keys.A && e.KeyCode <= Keys.Z)
            {
                if (textBox1.Text.Contains("Control") || textBox1.Text.Contains("Alt") || textBox1.Text.Contains("Shift"))
                {
                    if (arm_letter_entered == false)
                    {
                        textBox1.Text += "" + e.KeyCode.ToString() + "";
                        arm_letter_entered = true;
                    }
                }
                else if (arm_letter_entered == true)
                {
                    // empty on purpose
                }
                else
                {
                    toolTip1.Show("You must first press either Control, Alt, or Shift.", textBox1);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            arm_letter_entered = false;
            textBox1.Text = "";
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            String textbox_arm = textBox1.Text.Replace("  ", " ");
            textbox_arm = textbox_arm.Trim();

            String[] new_arm_kbd = textbox_arm.Split(Char.Parse(" "));
            String new_arm_shortcut = "";
            foreach (String kbd_item in new_arm_kbd)
            {
                new_arm_shortcut = kbd_item + ",";
            }
            new_arm_shortcut = new_arm_shortcut.Remove(new_arm_shortcut.Length - 1);
            Properties.Settings.Default.ArmShortcut = new_arm_shortcut;
            Properties.Settings.Default.Save();
        }

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
                groupBox5.Enabled = true;
            }
            else
            {
                groupBox5.Enabled = false;
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                button2.Enabled = true;
                Properties.Settings.Default.onalarm_audio_default = onalarm_audio_settings.customSound;
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

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked)
            {
                Properties.Settings.Default.onalarm_audio_volincrease = true;
            }
            else
            {
                Properties.Settings.Default.onalarm_audio_volincrease = false;
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

            if (Properties.Settings.Default.trigger_power)
            {
                stopBatProcess = false;
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
            if (workerVolThread.IsAlive == true)
            {
                workerVolThread.Abort();
            }
            if (workerBatThread.IsAlive == true)
            {
                workerBatThread.Abort();
            }
            alarmForm.CloseForm();
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
                               myAlarm.causeAlarm();
                               stopVolProcess = false;
                               workerVolThread = new Thread(new ThreadStart(setVolume));
                               workerVolThread.Start();
                               notifyIcon2.ShowBalloonTip(1000, "ALARM", "USB Device Removed at " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString(), ToolTipIcon.Warning);
                               alarmForm = new Form2("ALARM: USB Device removed at " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
                               alarmForm.Show();
                            }
                            break;
                    }
                    break;
            }
        }

        private void batteryAlarm()
        {
            myAlarm.causeAlarm();
            stopVolProcess = false;
            workerVolThread = new Thread(new ThreadStart(setVolume));
            workerVolThread.Start();
            notifyIcon2.ShowBalloonTip(1000, "ALARM", "AC adapter unplugged at " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString(), ToolTipIcon.Warning);
            alarmForm = new Form2("ALARM: AC adapter unplugged at " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
            alarmForm.Show();
            workerBatThread.Abort();
        }

        // battery detector
        private Thread workerBatThread = null;
        private bool stopBatProcess;
        private PowerStatus powerStatus = SystemInformation.PowerStatus;
        private void monitorBattery()
        {
            while (stopBatProcess == false)
            {
                if (powerStatus.PowerLineStatus != PowerLineStatus.Online)
                {
                    if (alarmArmed && Properties.Settings.Default.trigger_power)
                    {
                        batteryAlarm();
                    }
                }
                Thread.Sleep(1000);
            }
            workerBatThread.Abort();
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
                playbackDevice.Volume = 50;
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

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control == true)
            {
                if (!textBox2.Text.Contains("Control"))
                {
                    textBox2.Text += " Control ";
                }
            }
            else if (e.Alt == true)
            {
                if (!textBox2.Text.Contains("Alt"))
                {
                    textBox2.Text += " Alt ";
                }
            }
            else if (e.Shift == true)
            {
                if (!textBox2.Text.Contains("Shift"))
                {
                    textBox2.Text += " Shift ";
                }
            }
            else if (e.KeyCode >= Keys.A && e.KeyCode <= Keys.Z)
            {
                if (textBox2.Text.Contains("Control") || textBox2.Text.Contains("Alt") || textBox2.Text.Contains("Shift"))
                {
                    if (disarm_letter_entered == false)
                    {
                        textBox2.Text += "" + e.KeyCode.ToString() + "";
                        disarm_letter_entered = true;
                    }
                }
                else if (disarm_letter_entered == true)
                {
                    // empty on purpose
                }
                else
                {
                    toolTip1.Show("You must first press either Control, Alt, or Shift.", textBox2);
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            disarm_letter_entered = false;
            textBox2.Text = "";
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            toolStripMenuItem2.Enabled = false;
            toolStripMenuItem3.Enabled = true;
        }



        //// Volume set function
        //private void setVolume()
        //{
        //    if (playbackDevice.Volume != 20)
        //    {
        //        playbackDevice.Volume = 20;
        //    }
        //    if (playbackDevice.IsMuted)
        //    {
        //        playbackDevice.ToggleMute();
        //    }
        //    //playbackDevice.VolumeChanged += new EventHandler<AudioSwitcher.AudioApi.DeviceVolumeChangedArgs>(new VolumeChanged());
        //    playbackDevice.VolumeChanged.Subscribe(new VolumeChanged());
            
        //}

        //public class VolumeChanged : IObserver<AudioSwitcher.AudioApi.CoreAudio.CoreAudioDevice>
        //{
        //    private IDisposable unsubscriber;
        //    private bool first = true;
        //    private CoreAudioDevice last;

        //    public virtual void Subscribe(IObservable<CoreAudioDevice> provider)
        //    {
        //        unsubscriber = provider.Subscribe(this);
        //    }
        //    public virtual void Unsubscribe()
        //    {
        //        unsubscriber.Dispose();
        //    }
        //    public virtual void OnCompleted()
        //    {

        //    }
        //    public virtual void OnError(Exception e)
        //    {

        //    }
        //    public virtual void OnNext(CoreAudioDevice c)
        //    {

        //    }
        //}
    }
}
