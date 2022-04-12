﻿using System;
using System.Collections.Generic;
using System.Windows;

namespace Cliente
{
    /// <summary>
    /// Interaction logic for Chat.xaml
    /// </summary>
    public partial class Chat : Window
    {
        private string utilizador;

        public Chat(string username)
        {
            InitializeComponent();

            // Inicialização de variáveis

            utilizador = username;

            // Preencher elementos do UI

            textBlock_nomeUtilizador.Text = username;
            textBlock_listaUtilizadores.Text = "Eu"; // TODO: Ao longo do carregamento das informações do servidor, este campo deve ser alterado

            // Adição de notificação de entrada

            ServerNotificationControl joinNotification = new ServerNotificationControl("Ligado ao chat"); // TODO: trocar para mostrar apenas quando ligação for efetuada com sucesso
            messagePanel.Children.Add(joinNotification);
        }

        private void AdicionarMensagemCliente(string mensagem)
        {
            if (String.IsNullOrEmpty(mensagem) || String.IsNullOrWhiteSpace(mensagem))
                return;

            ClientMessageControl message = new ClientMessageControl(this.utilizador, DateTime.Now, mensagem);
            messagePanel.Children.Add(message);
        }

        private void AdicionarMensagemParticipante(string nomeParticipante, DateTime data, string mensagem)
        {
            MessageControl message = new MessageControl("Outro", data, mensagem);
            messagePanel.Children.Add(message);
        }

        private void button_enviarMensagem_Click(object sender, RoutedEventArgs e)
        {
            AdicionarMensagemCliente(textBox_mensagem.Text);
            textBox_mensagem.Clear();
            messagePanelScroll.ScrollToEnd(); // Exclusivo para quando a mensagem é do cliente atual
        }

        private void textBox_mensagem_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Enter)
            {
                AdicionarMensagemCliente(textBox_mensagem.Text);
                textBox_mensagem.Clear();
                messagePanelScroll.ScrollToEnd(); // Exclusivo para quando a mensagem é do cliente atual
            }
        }

        private void simularCliente_Click(object sender, RoutedEventArgs e)
        {
            // Botão temporário para gerar uma mensagem de outro cliente

            AdicionarMensagemParticipante("Outro", DateTime.Now, "Aliquam rutrum, magna at eleifend feugiat, nibh tortor tempor elit, non vehicula libero mauris sit amet felis. Aliquam egestas, tellus quis eleifend venenatis, ligula est scelerisque mauris, nec posuere turpis risus sit amet quam. Suspendisse sit amet lorem ut purus tristique dictum ut et diam. Pellentesque egestas cursus elit a finibus. Duis vulputate vulputate pharetra. Proin eu suscipit purus. Vivamus pharetra hendrerit nisi, in consectetur augue. Morbi finibus, mauris a viverra tempus, ante nulla pretium purus, a sagittis ipsum massa eu mauris.");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // Terminar processo
            Application.Current.Shutdown();
        }
    }
}