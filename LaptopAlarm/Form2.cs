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
    public partial class Form2 : Form
    {
        public bool form2_close = false;
        public Form2(String alarmDescription)
        {
            InitializeComponent();
            richTextBox1.Text = alarmDescription;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (form2_close == true)
            {
                this.Close();
            }
        }
    }
}
