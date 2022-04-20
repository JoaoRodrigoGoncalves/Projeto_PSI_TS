using System;
using System.Windows;
using System.Windows.Controls;

namespace Cliente
{
    /// <summary>
    /// Interaction logic for MessageControl.xaml
    /// </summary>
    public partial class MessageControl : UserControl
    {
        private bool profileLocked = false;


        public MessageControl(string username, DateTime messageDateTime, string message, int? maxWidth = null, bool blockProfile = false)
        {
            InitializeComponent();
            this.textBlock_nomeUtilizador.Text = username;
            this.textBlock_timestamp.Text = messageDateTime.ToString("G");
            this.textBlock_mensagem.Text = message;
            
            /// Esta condição é necessária por causa da janela <see cref="UserProfile"/>, de forma a que o controlo caiba no stackpanel
            if(maxWidth.HasValue)
                this.BallonBorder.MaxWidth = Convert.ToDouble(maxWidth);

            profileLocked = blockProfile;
        }

        private void textBlock_nomeUtilizador_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(!this.profileLocked)
            {
                UserProfile profile = new UserProfile(textBlock_nomeUtilizador.Text);
                profile.ShowDialog();
            }
        }

        private void textBlock_nomeUtilizador_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            textBlock_nomeUtilizador.TextDecorations = TextDecorations.Underline;
        }

        private void textBlock_nomeUtilizador_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            TextDecorationCollection decorations = new TextDecorationCollection();
            decorations.Clear();
            textBlock_nomeUtilizador.TextDecorations = decorations;
        }
    }
}
