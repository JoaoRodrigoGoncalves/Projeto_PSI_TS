using System.Windows.Controls;

namespace Cliente
{
    /// <summary>
    /// Interaction logic for ServerNotificationControl.xaml
    /// </summary>
    public partial class ServerNotificationControl : UserControl
    {
        public ServerNotificationControl(string mensagem)
        {
            InitializeComponent();
            this.textBlock_textoNotificacao.Text = mensagem;
        }
    }
}
