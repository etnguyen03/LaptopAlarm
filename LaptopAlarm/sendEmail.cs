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

        public sendEmail(String email, String smtp, bool ssl, String username, String password)
        {
            myEmail = email;
            mySmtp = smtp;
            mySSL = ssl;
            myUsername = username;
            myPassword = password;
        }
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
