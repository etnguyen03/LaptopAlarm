using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LaptopAlarm
{
    public enum onalarm_audio_settings { defaultSound, customSound };
    public partial class Form1 : Form
    {
        private bool allowVisible;
        private bool allowClose;
        private bool arm_letter_entered = false;
        private Alarm myAlarm;
        private bool alarmArmed;

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
            String[] armShortcut = new String[2];
            armShortcut = Properties.Settings.Default.ArmShortcut.Split(Convert.ToChar(","));
            foreach (String item in armShortcut)
            {
                if (item.Length == 1)
                {
                    
                }
            }

            if (allowVisible == false)
            {
                value = false;
                if (!this.IsHandleCreated)
                {
                    CreateHandle();
                }
            }
            base.SetVisibleCore(value);
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
                if (!textBox1.Text.Contains("Ctrl"))
                {
                    textBox1.Text += " Ctrl ";
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
                if (textBox1.Text.Contains("Ctrl") || textBox1.Text.Contains("Alt") || textBox1.Text.Contains("Shift"))
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
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog file_dialog = new OpenFileDialog();
            file_dialog.Filter = "Audio files (*.wav)|*.wav";
            if (file_dialog.ShowDialog() == DialogResult.OK)
            {
                Properties.Settings.Default.CustomAudioFilePath = file_dialog.FileName;
            }
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
            alarmArmed = true;
            notifyIcon2.ShowBalloonTip(100, "LaptopAlarm", "Armed", ToolTipIcon.Info);
        }

        // Disarm tool strip menu
        public void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            alarmArmed = false;
            myAlarm.stopAlarm();
            notifyIcon2.ShowBalloonTip(100, "LaptopAlarm", "Disarmed", ToolTipIcon.Info);
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
                               Form2 alarmForm = new Form2("ALARM: USB Device removed at " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
                                alarmForm.Show();
                            }
                            break;
                    }
                    break;
            }
        }
    }
}
