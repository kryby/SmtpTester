using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmtpTester
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            EnvioMail mail = new EnvioMail();
            string resultado = String.Empty;
            resultado = mail.envioNotificacion("Correo electronico de prueba", "Este es el texto del mensaje",
                txtDestinatario.Text, txtSmtpServer.Text, Convert.ToInt32(txtPuerto.Text), txtCuentaMail.Text,
                txtPassword.Text, chkSsl.Checked);
            rtResultado.AppendText(resultado);
        }
    }
}
