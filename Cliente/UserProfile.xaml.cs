using Core;
using EI.SI;
using Newtonsoft.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cliente
{
    public partial class UserProfile : Window
    {
        ProtocolSI protocolSI = new ProtocolSI();

        public UserProfile(uint id)
        {
            InitializeComponent();

            Basic_Packet pedidoMensagem = new Basic_Packet();
            pedidoMensagem.Type = PacketType.MESSAGE_HISTORY_REQUEST;
            pedidoMensagem.Contents = id;

            byte[] dados = protocolSI.Make(ProtocolSICmdType.SYM_CIPHER_DATA, Cryptography.AESEncrypt(Session.aes, JsonConvert.SerializeObject(pedidoMensagem)));
            Session.networkStream.Write(dados, 0, dados.Length);

            Session.networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length); // Ler o próximo pacote
            while (protocolSI.GetCmdType() != ProtocolSICmdType.SYM_CIPHER_DATA) // Enquanto não receber DATA (para ignorar ACKs e outros pacotes perdidos)
            {
                Session.networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length); // Ler o próximo pacote
            }

            Basic_Packet pacote = JsonConvert.DeserializeObject<Basic_Packet>(Cryptography.AESDecrypt(Session.aes, protocolSI.GetStringFromData()));

            if (pacote.Type == PacketType.MESSAGE_HISTORY_RESPONSE)
            {
                if (pacote.Contents != null)
                {
                    UserMessageHistory_Packet data = JsonConvert.DeserializeObject<UserMessageHistory_Packet>(pacote.Contents.ToString());

                    textBlock_nomeUtilizador.Text = data.username; // Mostrar o nome de utilizador
                    userImage.ImageSource = Utilities.getImage(data.userImage);// Mostra a imagem do utilizador atual

                    // Verifica se o utilizador está online ou não
                    if (UserManagement.GetUser(id) != null)
                    {
                        border_indicadorPresenca.Background = Brushes.LightGreen;// Cor do Indicador de Estado, se estiver online
                        textBlock_UltimaOnline.Text = "Agora";// Mensagem de estado, se estiver online
                    }
                    else
                    {
                        border_indicadorPresenca.Background = Brushes.LightGray;// Cor do Indicador de Estado, se estiver offline

                        if (data.messages != null)
                            textBlock_UltimaOnline.Text = data.messages[data.messages.Count - 1].time.ToString("G");
                    }

                    if (data.messages == null)
                    {
                        TextBlock advertencia = new TextBlock();
                        advertencia.Text = "Não existem mensagens!";
                        advertencia.HorizontalAlignment = HorizontalAlignment.Center;
                        advertencia.VerticalAlignment = VerticalAlignment.Center;
                        advertencia.FontSize = 12;
                        advertencia.Foreground = Brushes.Black;
                        messagePanel.Children.Add(advertencia);
                    }
                    else
                    {
                        foreach (UserMessageHistoryItem_Packet element in data.messages)
                        {
                            MessageControl messageControl = new MessageControl(id, data.username, data.userImage, element.time, element.message, 260, true);
                            messagePanel.Children.Add(messageControl);
                        }
                    }
                }
                else
                {
                    // Erro. O pacote não trouxe nada
                    MessageBox.Show("Erro: Conteúdo do pacote vazio");
                }
            }
        }
    }
}

