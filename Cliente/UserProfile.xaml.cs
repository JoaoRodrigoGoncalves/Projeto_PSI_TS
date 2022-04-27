using System;
using System.Collections.Generic;
using Core;
using EI.SI;
using Newtonsoft.Json;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;

namespace Cliente
{
    public partial class UserProfile : Window
    {
        ProtocolSI protocolSI = new ProtocolSI();

        public UserProfile(uint id)
        {
            InitializeComponent();

            UserInfo user = UserManagement.GetUser(id);

            if (user != null)
            {
                border_indicadorPresenca.Background = Brushes.Green;// Cor do Indicador de Estado, se estiver online
            }
            else
            {
                border_indicadorPresenca.Background = Brushes.LightGray;// Cor do Indicador de Estado, se estiver offline
            }


            Basic_Packet pedidoMensagem = new Basic_Packet();
            pedidoMensagem.Type = PacketType.MESSAGE_HISTORY_REQUEST;
            pedidoMensagem.Contents = id;

            byte[] dados = protocolSI.Make(ProtocolSICmdType.DATA, JsonConvert.SerializeObject(pedidoMensagem));
            Session.networkStream.Write(dados, 0, dados.Length);

            Session.networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length); // Ler o próximo pacote
            while (protocolSI.GetCmdType() != ProtocolSICmdType.DATA) // Enquanto não receber DATA (para ignorar ACKs e outros pacotes perdidos)
            {
                Session.networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length); // Ler o próximo pacote
            }

            Basic_Packet pacote = JsonConvert.DeserializeObject<Basic_Packet>(protocolSI.GetStringFromData());

            if (pacote.Type == PacketType.MESSAGE_HISTORY_RESPONSE)
            {
                if (pacote.Contents == null)
                {
                    TextBlock advertencia = new TextBlock();
                    advertencia.Text = "Não existem mensagens!";

                    messagePanel.Children.Add(advertencia);
                }
                else
                {
                    List<UserMessageHistoryItem_Packet> resposta_mensagem = JsonConvert.DeserializeObject<List<UserMessageHistoryItem_Packet>>(pacote.Contents.ToString());
                
                    foreach(UserMessageHistoryItem_Packet element in resposta_mensagem)
                    {
                        MessageControl messageControl = new MessageControl(user.username, element.time, element.message, 260, true);
                        messagePanel.Children.Add(messageControl);
                    }
                }
            }
        }
    }
}

