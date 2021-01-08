using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SmtpTester
{
    class EnvioMail

    {
     

        public string envioNotificacion(string Asunto, string texto, string destinatario, string servidor, 
            int puerto, string usuario, string pass, bool enableSSl)
        {
            try
            {
                SmtpClient client = new SmtpClient();
                client.Port = puerto;
                client.Host = servidor;
                client.EnableSsl = enableSSl;
                
                client.Timeout = 10000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(usuario, pass);

                MailMessage mm = new MailMessage(usuario, destinatario, Asunto, texto);
                mm.IsBodyHtml = true;
                mm.BodyEncoding = UTF8Encoding.UTF8;
                mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                client.Send(mm);
                return "Envio OK";
            }
            catch (System.Exception E)
            {
                return E.Message;
                throw;
            }
        }
    }
}
