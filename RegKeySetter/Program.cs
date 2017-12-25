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
using Microsoft.Win32;

namespace RegKeySetter
{
    class Program
    {
        static void Main(string[] args)
        {
            RegistryKey regKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            // helps to ensure that program not run by end user and instead triggered by main program
            try
            {
                if (args[0] == "7FS0SDE4QM2NOQKYSKPCD5LUCRGK6ZV4" && args[1] == "9F78IPHB517W4AJ1H3VKEY3AOJ9M8CWG" && args[2] == "C8JU5GAQZOVGX02HD6LC85T4TS0H8JRH")
                {
                    if (args[3] == "1")
                    {
                        // set registry key
                        regKey.SetValue("LaptopAlarm", args[4]);
                    }
                    else if (args[3] == "0")
                    {
                        // remove key
                        regKey.DeleteValue("LaptopAlarm");
                    }
                }
            }
            catch (IndexOutOfRangeException)
            {
                throw new Exception("This program must be called by LaptopAlarm.");
            }
            catch (UnauthorizedAccessException)
            {
                throw new Exception("This program must be run with elevated permissions.");
            }
        }
    }
}
