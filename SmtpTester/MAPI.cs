

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SmtpTester
{
    class MAPI
        {
            string firmaCorreo = "\n\n-------------------------------------------------\n" +
                               "Notaría D.JOSE MIGUEL GARCIA LOMBARDIA\n" +
                               "C/ José Ortega y Gasset, 5.Izq\n" +
                               "28006 Madrid\n" +
                               "Teléfono: 91.781.71.70\n" +
                               "Fax: 91.781.71.80" +
                               "\nNota de confidencialidad: Este mensaje se envía desde el sistema de correo electrónico de la notaria de José Miguel García Lombardía.\n" +
                               "Podría contener por lo tanto secretos de empresa y otra información confidencial tutelados por las leyes de la Unión Europea y otros Estados.\n" +
                               "Si Vd. lo recibiera por error o sin ser una de las personas indicadas en el encabezamiento, deberá destruirlo sin copiarlo ni revelarlo o cualquier otra acción relacionada con el contenido del mensaje\n" +
                               "e informar inmediatamente por correo electrónico al emisor del mensaje.\n" +
                               "Las personas y entidades que violen sus deberes de confidencialidad podrán ser perseguidas ante los Tribunales de Justicia al amparo de la legislación civil, penal y administrativa nacional e internacional.\n" +
                               "Gracias por su colaboración.";

            public bool AddRecipientTo(string email)
            {
                return AddRecipient(email, HowTo.MAPI_TO);
            }

            public bool AddRecipientCC(string email)
            {
                return AddRecipient(email, HowTo.MAPI_TO);
            }

            public bool AddRecipientBCC(string email)
            {
                return AddRecipient(email, HowTo.MAPI_TO);
            }

            public void AddAttachment(string strAttachmentFileName)
            {
                m_attachments.Add(strAttachmentFileName);
            }

            public int SendMailPopup(string strSubject, string strBody)
            {
                strBody = strBody + "\n" + firmaCorreo;
                return SendMail(strSubject, strBody, MAPI_LOGON_UI | MAPI_DIALOG);
            }

            public int SendMailDirect(string strSubject, string strBody)
            {
                return SendMail(strSubject, strBody, MAPI_LOGON_UI);
            }


            [DllImport("MAPI32.DLL")]
            static extern int MAPISendMail(IntPtr sess, IntPtr hwnd,
                MapiMessage message, int flg, int rsv);

            int SendMail(string strSubject, string strBody, int how)
            {
                MapiMessage msg = new MapiMessage();
                msg.subject = strSubject;
                msg.noteText = strBody;

                msg.recips = GetRecipients(out msg.recipCount);
                msg.files = GetAttachments(out msg.fileCount);

                m_lastError = MAPISendMail(new IntPtr(0), new IntPtr(0), msg, how,
                    0);
                if (m_lastError > 1)
                    MessageBox.Show("Error generando el correo de aviso: \n Por favor, cierre Outlook y vuelva a intentarlo. " + GetLastError(),
                        "MAPISendMail");

                Cleanup(ref msg);
                return m_lastError;
            }

            bool AddRecipient(string email, HowTo howTo)
            {
                MapiRecipDesc recipient = new MapiRecipDesc();

                recipient.recipClass = (int)howTo;
                recipient.name = email;
                m_recipients.Add(recipient);

                return true;
            }

            IntPtr GetRecipients(out int recipCount)
            {
                recipCount = 0;
                if (m_recipients.Count == 0)
                    return IntPtr.Zero;

                int size = Marshal.SizeOf(typeof(MapiRecipDesc));
                IntPtr intPtr = Marshal.AllocHGlobal(m_recipients.Count * size);

                int ptr = (int)intPtr;
                foreach (MapiRecipDesc mapiDesc in m_recipients)
                {
                    Marshal.StructureToPtr(mapiDesc, (IntPtr)ptr, false);
                    ptr += size;
                }

                recipCount = m_recipients.Count;
                return intPtr;
            }

            IntPtr GetAttachments(out int fileCount)
            {
                fileCount = 0;
                if (m_attachments == null)
                    return IntPtr.Zero;

                if ((m_attachments.Count <= 0) || (m_attachments.Count >
                    maxAttachments))
                    return IntPtr.Zero;

                int size = Marshal.SizeOf(typeof(MapiFileDesc));
                IntPtr intPtr = Marshal.AllocHGlobal(m_attachments.Count * size);

                MapiFileDesc mapiFileDesc = new MapiFileDesc();
                mapiFileDesc.position = -1;
                int ptr = (int)intPtr;

                foreach (string strAttachment in m_attachments)
                {
                    mapiFileDesc.name = Path.GetFileName(strAttachment);
                    mapiFileDesc.path = strAttachment;
                    Marshal.StructureToPtr(mapiFileDesc, (IntPtr)ptr, false);
                    ptr += size;
                }

                fileCount = m_attachments.Count;
                return intPtr;
            }

            void Cleanup(ref MapiMessage msg)
            {
                int size = Marshal.SizeOf(typeof(MapiRecipDesc));
                int ptr = 0;

                if (msg.recips != IntPtr.Zero)
                {
                    ptr = (int)msg.recips;
                    for (int i = 0; i < msg.recipCount; i++)
                    {
                        Marshal.DestroyStructure((IntPtr)ptr,
                            typeof(MapiRecipDesc));
                        ptr += size;
                    }
                    Marshal.FreeHGlobal(msg.recips);
                }

                if (msg.files != IntPtr.Zero)
                {
                    size = Marshal.SizeOf(typeof(MapiFileDesc));

                    ptr = (int)msg.files;
                    for (int i = 0; i < msg.fileCount; i++)
                    {
                        Marshal.DestroyStructure((IntPtr)ptr,
                            typeof(MapiFileDesc));
                        ptr += size;
                    }
                    Marshal.FreeHGlobal(msg.files);
                }

                m_recipients.Clear();
                m_attachments.Clear();
                m_lastError = 0;
            }

            public string GetLastError()
            {
                if (m_lastError <= 26)
                    return errors[m_lastError];
                return "MAPI error [" + m_lastError.ToString() + "]";
            }

            readonly string[] errors = new string[] {
    "OK [0]", "Cancelado por el usuario [1]", "Error mapi [2]",
            "Error login mapi [3]", "Disco sin espacio [4]",
            "Memoria insuficiente [5]", "Acceso denegado [6]",
            "Error desconocido [7]", "Demasiadas sesiones [8]",
            "Demasiados archivos adjuntos [9]",
            "Demasiados destinatarios [10]",
            "No se ha encontrado un archivo adjunto [11]",
    "Fallo al adjuntar archivo [12]",
            "Fallo al escribir archivo [13]", "Destinatario desconocido[14]",
            "Destinatario erroneo [15]", "Sin mensajes [16]",
            "Formato de mensaje incorrecto [17]", "Texto demasido largo [18]",
            "Sesion incorrecta [19]", "Tipo no soportado [20]",
            "Destinatario incorrecto [21]",
            "Mensaje en uso[22]", "Error de red[23]",
    "Campos no validos [24]", "Destinatarios incorrectos [25]",
            "No soportado [26]"
    };


            List<MapiRecipDesc> m_recipients = new
                List<MapiRecipDesc>();
            List<string> m_attachments = new List<string>();
            int m_lastError = 0;

            const int MAPI_LOGON_UI = 0x00000001;
            const int MAPI_DIALOG = 0x00000008;


            const int maxAttachments = 100;

            enum HowTo { MAPI_ORIG = 0, MAPI_TO, MAPI_CC, MAPI_BCC };
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class MapiMessage
        {
            public int reserved;
            public string subject;
            public string noteText;
            public string messageType;
            public string dateReceived;
            public string conversationID;
            public int flags;
            public IntPtr originator;
            public int recipCount;
            public IntPtr recips;
            public int fileCount;
            public IntPtr files;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class MapiFileDesc
        {
            public int reserved;
            public int flags;
            public int position;
            public string path;
            public string name;
            public IntPtr type;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class MapiRecipDesc
        {
            public int reserved;
            public int recipClass;
            public string name;
            public string address;
            public int eIDSize;
            public IntPtr entryID;
        }



    }

