﻿using System;
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

        // enums
        

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
    }
}
