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
    public partial class Form2 : Form
    {
        public Form2(String alarmDescription)
        {
            InitializeComponent();
            richTextBox1.Text = alarmDescription;
        }

        public void Close()
        {
            this.Close();
        }
    }
}