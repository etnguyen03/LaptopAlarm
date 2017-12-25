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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;

namespace LaptopAlarm
{
    class sendEmail
    {
        private string myEmail, mySmtp, myUsername, myPassword;
        private bool mySSL;

        private const string Subject = "LaptopAlarm - Alarm Triggered";
        private const string messageIntro = "Message from LaptopAlarm: Alarm triggered! Details: ";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="email">Email address</param>
        /// <param name="smtp">SMTP address</param>
        /// <param name="ssl">Whether or not to use SSL</param>
        /// <param name="port">Port number</param>
        /// <param name="username">Username for SMTP server</param>
        /// <param name="password">Password (plaintext) for SMTP server</param>
        public sendEmail(String email, String smtp, bool ssl, int port, String username, String password)
        {
            myEmail = email;
            mySmtp = smtp;
            mySSL = ssl;
            myUsername = username;
            myPassword = password;
        }

        /// <summary>
        /// Send an email
        /// </summary>
        /// <param name="alarmDescription">Alarm description to include in email.</param>
        public void sendtheEmail(String alarmDescription = "NO DETAILS PROVIDED")
        {
            var fromAddress = new MailAddress(myEmail);
            var toAddress = new MailAddress(myEmail);
            int port;

            if (mySSL)
            {
                port = 587;
            }
            else
            {
                port = 25;
            }

            var smtp = new SmtpClient();
            if (myUsername == "" && myPassword == "")
            {
                smtp = new SmtpClient
                {
                    Host = mySmtp,
                    Port = port,
                    EnableSsl = mySSL,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false
                };
            }
            else
            {
                smtp = new SmtpClient
                {
                    Host = mySmtp,
                    Port = port,
                    EnableSsl = mySSL,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(myUsername, myPassword)
                };
            }

            using (var message = new MailMessage(fromAddress, toAddress){
                Subject = Subject,
                Body = messageIntro + alarmDescription
            })
            {
                smtp.Send(message);
            }
        }
    }
}
