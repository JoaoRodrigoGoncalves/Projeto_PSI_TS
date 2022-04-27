using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net.Sockets;
using System.Windows;
using Core;
using EI.SI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace Cliente
{
    /// <summary>
    /// Interaction logic for Chat.xaml
    /// </summary>
    public partial class Chat : Window
    {
        ProtocolSI protocolSI = new ProtocolSI();
        public Chat()
        {
            InitializeComponent();
            
            // Pedir lista de utilizadores ativos

            // Preencher elementos do UI
            textBlock_nomeUtilizador.Text = Session.username;
            // TODO: Ao longo do carregamento das informações do servidor, este campo deve ser alterado
            Basic_Packet requestUserOn = new Basic_Packet();
            requestUserOn.Type= PacketType.USER_LIST_REQUEST;

            requestUserOn.Contents = null;

            byte[] dados = protocolSI.Make(ProtocolSICmdType.DATA, JsonConvert.SerializeObject(requestUserOn));
            Session.networkStream.Write(dados, 0, dados.Length);

            // Esperar e ler os dados enviados pelo servidor. Caso existam utilizadores online são mostrados na textBlock_listaUtilizadores
            // Ler resposta do tipo USER_LIST_REQUEST
            Session.networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length); // Ler o próximo pacote
            while (protocolSI.GetCmdType() != ProtocolSICmdType.DATA) // Enquanto não receber DATA (para ignorar ACKs e outros pacotes perdidos)
            {
                Session.networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length); // Ler o próximo pacote
            }
             
            Basic_Packet pacote = JsonConvert.DeserializeObject<Basic_Packet>(protocolSI.GetStringFromData());
            
            List<UserListItem_Packet> packets = JsonConvert.DeserializeObject<List<UserListItem_Packet>>(pacote.Contents.ToString());
            foreach(var users in packets)
            {
                textBlock_listaUtilizadores.Text +=users.username +" ";
            }
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
