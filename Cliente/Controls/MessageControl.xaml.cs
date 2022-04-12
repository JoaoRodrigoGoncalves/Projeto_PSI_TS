using System;
using System.Windows.Controls;

namespace Cliente
{
    /// <summary>
    /// Interaction logic for MessageControl.xaml
    /// </summary>
    public partial class MessageControl : UserControl
    {
        public MessageControl(string username, DateTime messageDateTime, string message)
        {
            InitializeComponent();
            this.textBlock_nomeUtilizador.Text = username;
            this.textBlock_timestamp.Text = messageDateTime.ToString("G");
            this.textBlock_mensagem.Text = message;
        }
    }
}
