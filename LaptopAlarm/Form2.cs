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
