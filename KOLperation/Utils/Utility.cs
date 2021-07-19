using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web;

namespace KOLperation.Utils
{
    public class Utility
    {
        public static bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
        }

        public static bool IsValidCellnumber(string phoneNumber)
        {
            if (String.IsNullOrEmpty(phoneNumber) || phoneNumber.Length != 10)
            {
                return false;
            }
            else
            {
                if (phoneNumber.Substring(0, 2) != "09")
                {
                    return false;
                }
                foreach (char chr in phoneNumber)
                {
                    if (!Char.IsDigit(chr))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        // Reference:https://dotblogs.com.tw/ChentingW/2020/03/29/000036
        public static bool IsValidTaxIdNumber(string taxIdNumber)
        {
            if (taxIdNumber == null)
            {
                return false;
            }
            Regex regex = new Regex(@"^\d{8}$");
            Match match = regex.Match(taxIdNumber);
            if (!match.Success)
            {
                return false;
            }
            int[] idNoArray = taxIdNumber.ToCharArray().Select(c => Convert.ToInt32(c.ToString())).ToArray();
            int[] weight = new int[] { 1, 2, 1, 2, 1, 2, 4, 1 };

            int subSum;     //小和
            int sum = 0;    //總和
            int sumFor7 = 1;
            for (int i = 0; i < idNoArray.Length; i++)
            {
                subSum = idNoArray[i] * weight[i];
                sum += (subSum / 10)   //商數
                     + (subSum % 10);  //餘數
            }
            if (idNoArray[6] == 7)
            {
                //若第7碼=7，則會出現兩種數值都算對，因此要特別處理。
                sumFor7 = sum + 1;
            }
            return (sum % 10 == 0) || (sumFor7 % 10 == 0);
        }

        public static void SendHtmlFormattedEmail(string recepientEmail, string subject, string body)
        {
            using (MailMessage mailMessage = new MailMessage())
            {
                mailMessage.From = new MailAddress(ConfigurationManager.AppSettings["UserName"], "KOLperation", System.Text.Encoding.UTF8);
                mailMessage.BodyEncoding = System.Text.Encoding.UTF8;
                mailMessage.Subject = subject;
                mailMessage.BodyEncoding = System.Text.Encoding.UTF8;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = true;
                mailMessage.To.Add(new MailAddress(recepientEmail));
                SmtpClient smtp = new SmtpClient();
                smtp.Host = ConfigurationManager.AppSettings["Host"].ToString();
                smtp.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSsl"]);
                NetworkCredential NetworkCred = new NetworkCredential();
                NetworkCred.UserName = ConfigurationManager.AppSettings["UserName"].ToString();
                NetworkCred.Password = ConfigurationManager.AppSettings["Password"].ToString();
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = NetworkCred;
                smtp.Port = Convert.ToInt16(ConfigurationManager.AppSettings["Port"]);
                smtp.Send(mailMessage);
            }
        }

        public static string PopulateBody(string userName, string content, string link, string description)
        {
            string path = HttpContext.Current.Server.MapPath("~/Utils/Email.html");
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(path))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{UserName}", userName);
            body = body.Replace("{Content}", content);
            body = body.Replace("{Link}", link);
            body = body.Replace("{Description}", description);
            return body;
        }
    }
}