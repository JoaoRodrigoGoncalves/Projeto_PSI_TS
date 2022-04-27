using System;
using System.Net.Sockets;
using System.Windows;
using Core;

namespace Cliente
{
    /// <summary>
    /// Interaction logic for Chat.xaml
    /// </summary>
    public partial class Chat : Window
    {
        public Chat()
        {
            InitializeComponent();

            // Pedir lista de utilizadores ativos

            // Preencher elementos do UI
            textBlock_nomeUtilizador.Text = Session.username;
            textBlock_listaUtilizadores.Text = null;
            // TODO: Ao longo do carregamento das informações do servidor, este campo deve ser alterado

            // Adição de notificação de entrada

            ServerNotificationControl joinNotification = new ServerNotificationControl("Ligado ao chat"); // TODO: trocar para mostrar apenas quando ligação for efetuada com sucesso
            messagePanel.Children.Add(joinNotification);
        }

        private void adicionarMensagemCliente(string mensagem)
        {
            if (String.IsNullOrWhiteSpace(mensagem))
                return;

            ClientMessageControl message = new ClientMessageControl(Session.username, DateTime.Now, mensagem);
            messagePanel.Children.Add(message);
        }

        private void adicionarMensagemParticipante(string nomeParticipante, DateTime data, string mensagem)
        {
            //MessageControl message = new MessageControl("Outro", data, mensagem);
            //messagePanel.Children.Add(message);
        }

        private void button_enviarMensagem_Click(object sender, RoutedEventArgs e)
        {
            adicionarMensagemCliente(textBox_mensagem.Text);
            textBox_mensagem.Clear();
            messagePanelScroll.ScrollToEnd(); // Exclusivo para quando a mensagem é do cliente atual
        }

        private void textBox_mensagem_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Enter)
            {
                adicionarMensagemCliente(textBox_mensagem.Text);
                textBox_mensagem.Clear();
                messagePanelScroll.ScrollToEnd(); // Exclusivo para quando a mensagem é do cliente atual
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // Terminar processo
            Application.Current.Shutdown();
        }

        private void button_terminarSessao_Click(object sender, RoutedEventArgs e)
        {
            //Termina a sessão do cliente e mostra a janela de login
            if (Session.Client != null)
            {
                this.Hide();
                Session.CloseTCPSession();
                Login loginjanela = new Login();
                loginjanela.ShowDialog();
            } 
        }
    }
}
