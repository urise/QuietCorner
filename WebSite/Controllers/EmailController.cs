using ActionMailer.Net.Mvc;
using CommonClasses.Helpers;
using CommonClasses.InfoClasses;

namespace WebSite.Controllers
{
    public class EmailController : MailerBase
    {
        public string ViewName { get; set; }
        public PasswordMailInfo Model { get; set; }

        public EmailController(string viewName, PasswordMailInfo model)
        {
            ViewName = viewName;
            Model = model;
        }

        public EmailResult SendEmailBase(string to, string from, string subject)
        {
            To.Add(to);
            From = from;
            Subject = subject;
            return Email(ViewName, Model);
        }

        public void SendConfirmEmail()
        {
            if (Model != null)
            {
                var mail = SendEmailBase(Model.Email, AppConfiguration.AdminEmailAddress, "Confirm Email");
                mail.Deliver();
            }
        }

        public void SendPasswordEmail()
        {
            if (Model != null)
            {
                var mail = SendEmailBase(Model.Email, AppConfiguration.AdminEmailAddress, "Password Email");
                mail.Deliver();
            }
        }
    }
}
