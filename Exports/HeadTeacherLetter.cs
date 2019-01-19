using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using FConn.Reporting;
using ChampionshipSolutions.DM;
using System.Threading.Tasks;
using System.Data;

namespace ChampionshipSolutions.ControlRoom
{
    public class HeadTeacherLetter
    {

        public static void SendEmail(HeadTeacherLetter htl)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("mail.fconn.co.uk");
                mail.From = new MailAddress("noreply@championship.solutions");

                htl.EmailAddresses.ForEach(e => mail.To.Add(e));
                mail.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                mail.Bcc.Add("company@fconn.co.uk");
                mail.CC.Add("Tim Whiting <klondike_tim@hotmail.com>");

                //mail.To.Add("fraser@fconn.co.uk");
                mail.Subject = @Properties.Settings.Default.EmailSubject ;

                string body = @Properties.Settings.Default.EmailBody;

                   body = body.Replace("*", Environment.NewLine);

                mail.Body = body;

                System.Net.Mail.Attachment attachment;
                attachment = new System.Net.Mail.Attachment(htl.LetterFilePath);
                mail.Attachments.Add(attachment);

                SmtpServer.Port = 25;
                SmtpServer.Credentials = new System.Net.NetworkCredential("catch@fconn.co.uk", "Hornby37");
                SmtpServer.EnableSsl = false;

                //SmtpServer.Send(mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw new ArgumentException("Failed to send email as one or more addresses is invalid");
            }
        }

        public static void GenerateLetter(HeadTeacherLetter htl)
        {
            //// changed to work for track and field. 2015-06-16
            //int count = htl.School.getSelectedForNextEvent(htl.Championship).Count;
            
            //PDFReport letter;

            //if (count == 0) return;
            //if (count == 1)
            //    letter = (PDFReport)PDFReport.LoadTemplate(@Properties.Settings.Default.OLD_RootPathTemplate + "2014-15 TF SW Head Teachers Letter Single");
            //else
            //    letter = (PDFReport)PDFReport.LoadTemplate(@Properties.Settings.Default.OLD_RootPathTemplate + "2014-15 TF SW Head Teachers Letter");

            //DataTable dataTable = new DataTable();

            //dataTable.Columns.Add("Athletes1");
            //dataTable.Columns.Add("Athletes2");
            //dataTable.Columns.Add("Athletes3");
            //dataTable.Columns.Add("Cost");
            //dataTable.Columns.Add("Count");

            //string athletesNames1 = string.Empty;
            //string athletesNames2 = string.Empty;
            //string athletesNames3 = string.Empty;

            //foreach (ACompetitor c in htl.School.getSelectedForNextEvent(htl.Championship).Take(4))
            //    athletesNames1 += c.Name + Environment.NewLine;

            //foreach (ACompetitor c in htl.School.getSelectedForNextEvent(htl.Championship).Except(htl.School.getSelectedForNextEvent(htl.Championship).Take(4)).Take(4))
            //    athletesNames2 += c.Name + Environment.NewLine;

            //foreach (ACompetitor c in htl.School.getSelectedForNextEvent(htl.Championship).Except(htl.School.getSelectedForNextEvent(htl.Championship).Take(8)).Take(4))
            //    athletesNames3 += c.Name + Environment.NewLine;


            //DataRow dr = dataTable.NewRow();

            //dr["Athletes1"] = athletesNames1;
            //dr["Athletes2"] = athletesNames2;
            //dr["Athletes3"] = athletesNames3;
            //dr["Cost"] = (Properties.Settings.Default.CostPerSWAthlete * htl.School.CountSelected(htl.Championship));
            //dr["Count"] = count;

            //dataTable.Rows.Add(dr);

            //letter.Generate(dataTable);
            //letter.Save(@Properties.Settings.Default.HeadTeachersExportPath + htl.School.Name + ".pdf");
            //htl.LetterFilePath = @Properties.Settings.Default.HeadTeachersExportPath + htl.School.Name + ".pdf";
        }

        public virtual School School { get; set; }
        public virtual Championship Championship { get; set; }
        public string LetterFilePath { get; set; }

        public List<string> EmailAddresses
        {
            get
            {
                List<string> strs = new List<string>();

                foreach (AContactDetail cd in School.getAllContacts())
                    if (cd.GetType().ToString() == "ChampionshipSolutions.DM.EmailContactDetail")
                        strs.Add(((EmailContactDetail)cd).printValue);

                return strs;
            }
        }

    }



}
