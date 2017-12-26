using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Principal;

namespace LaptopAlarm
{
    public partial class passwordBox : Form
    {
        bool okClicked = false;
        public passwordBox()
        {
            InitializeComponent();
        }

        private void passwordBox_Load(object sender, EventArgs e)
        {
            textBox1.Text = WindowsIdentity.GetCurrent().Name;
        }

        /// <summary>
        /// Get the username entered in the dialog.
        /// </summary>
        /// <returns>Username.</returns>
        public string getUsername()
        {
            return textBox1.Text;
        }

        /// <summary>
        /// Get the password entered in the dialog.
        /// </summary>
        /// <returns>Password.</returns>
        public string getPassword()
        {
            return textBox2.Text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            okClicked = true;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void passwordBox_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (okClicked == false)
            {
                this.DialogResult = DialogResult.Cancel;
            }
        }
    }
}
