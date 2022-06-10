using Core;
using EI.SI;
using Newtonsoft.Json;
using System;
using System.Windows;
using System.Windows.Media;

namespace Cliente
{
    /// <summary>
    /// Interaction logic for registar.xaml
    /// </summary>
    public partial class Registar : Window
    {
        ProtocolSI protocolSI = new ProtocolSI();
        //private static string imageBase64 = null;

        public Registar()
        {
            InitializeComponent();
        }

        private void button_registar_Click(object sender, RoutedEventArgs e)
        {
            if (Session.Client == null)
                Session.StartTCPSession();

            if (!String.IsNullOrWhiteSpace(textBox_nomeUtilizador.Text))
            {
                if (!String.IsNullOrWhiteSpace(textBox_palavraPasse.Password))
                {
                    if (textBox_palavraPasse.Password == textBox_verificarPalavraPasse.Password)
                    {

                        //Envia os dados do utilizador(username e password) para o servidor 
                        Basic_Packet pedidoRegisto = new Basic_Packet();
                        pedidoRegisto.Type = PacketType.REGISTER_REQUEST;//Indicar que o pacote é um pedido de registo

                        Register_Request_Packet registo = new Register_Request_Packet();
                        registo.username = textBox_nomeUtilizador.Text;
                        registo.password = textBox_palavraPasse.Password;

                        uint? imageProfile = null;

                        if (image1.IsChecked == true)
                        {
                            imageProfile = 1;
                        }
                        else if (image2.IsChecked == true)
                        {
                            imageProfile = 2;
                        }
                        else if (image3.IsChecked == true)
                        {
                            imageProfile = 3;
                        }
                        else if (image4.IsChecked == true)
                        {
                            imageProfile = 4;
                        }

                        registo.userImage = imageProfile;

                        pedidoRegisto.Contents = registo;

                        byte[] dados = protocolSI.Make(ProtocolSICmdType.SYM_CIPHER_DATA, Cryptography.AESEncrypt(Session.aes, JsonConvert.SerializeObject(pedidoRegisto)));
                        Session.networkStream.Write(dados, 0, dados.Length);

                        Session.networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length); // Ler o próximo pacote
                        while (protocolSI.GetCmdType() != ProtocolSICmdType.SYM_CIPHER_DATA) // Enquanto não receber DATA (para ignorar ACKs e outros pacotes perdidos)
                        {
                            Session.networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length); // Ler o próximo pacote
                        }

                        Basic_Packet pacote = JsonConvert.DeserializeObject<Basic_Packet>(Cryptography.AESDecrypt(Session.aes, protocolSI.GetStringFromData()));

                        if (pacote.Type == PacketType.REGISTER_RESPONSE)
                        {
                            Register_Response_Packet resposta_registo = JsonConvert.DeserializeObject<Register_Response_Packet>(pacote.Contents.ToString());

                            if (resposta_registo.success)// Boolean: Se o registo foi feito com sucesso
                            {
                                //Janela de Mensagem de um registo bem sucedido 
                                string messageBoxText = "O utilizador foi registado com sucesso!";
                                string caption = "Registo";
                                MessageBoxButton button = MessageBoxButton.OK;
                                MessageBoxImage icon = MessageBoxImage.Information;
                                MessageBoxResult result;

                                result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);

                                //Depois de carregar "OK", da janela de mensagem, a janela de registo é fechada 
                                this.Close();
                            }
                            else
                            {
                                textBlock_ErrorMessage.Text = "Erro! " + resposta_registo.message; // Mostrar mensagem de erro do servidor
                            }
                        }
                    }
                    else
                    {
                        textBlock_ErrorMessage.Text = "As palavras-passse não coincidem.";
                        textBox_palavraPasse.BorderBrush = Brushes.Red;
                        textBox_verificarPalavraPasse.BorderBrush = Brushes.Red;
                    }
                }
                else
                {
                    textBox_palavraPasse.BorderBrush = Brushes.Red;
                }
            }
            else
            {
                textBox_nomeUtilizador.BorderBrush = Brushes.Red;
            }
        }
    }
}
