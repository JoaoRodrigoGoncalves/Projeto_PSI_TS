using System;
using EI.SI;
using System.Windows;
using System.Windows.Media;
using Core;
using Newtonsoft.Json;
using Microsoft.Win32;
using System.Windows.Media.Imaging;

namespace Cliente
{
    /// <summary>
    /// Interaction logic for registar.xaml
    /// </summary>
    public partial class Registar : Window
    {
        ProtocolSI protocolSI = new ProtocolSI();
        private static string imageBase64 = null;

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
                    if (textBox_palavraPasse.Password == textBox_verificarPalavraPasse.Password) {
                    
                        //Envia os dados do utilizador(username e password) para o servidor 
                        Basic_Packet pedidoRegisto = new Basic_Packet();
                        pedidoRegisto.Type = PacketType.REGISTER_REQUEST;//Indicar que o pacote é um pedido de registo

                        Register_Request_Packet registo = new Register_Request_Packet();
                        registo.username = textBox_nomeUtilizador.Text;
                        registo.password = textBox_palavraPasse.Password;
                        registo.password = textBox_verificarPalavraPasse.Password;
                        registo.userImage = imageBase64;

                        pedidoRegisto.Contents = registo;

                        byte[] dados = protocolSI.Make(ProtocolSICmdType.DATA, JsonConvert.SerializeObject(pedidoRegisto));
                        Session.networkStream.Write(dados, 0, dados.Length);

                        Session.networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length); // Ler o próximo pacote
                        while (protocolSI.GetCmdType() != ProtocolSICmdType.DATA) // Enquanto não receber DATA (para ignorar ACKs e outros pacotes perdidos)
                        {
                            Session.networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length); // Ler o próximo pacote
                        }

                        Basic_Packet pacote = JsonConvert.DeserializeObject<Basic_Packet>(protocolSI.GetStringFromData());

                        //Close();

                        if (pacote.Type == PacketType.REGISTER_RESPONSE)
                        {
                            Register_Response_Packet resposta_registo = JsonConvert.DeserializeObject<Register_Response_Packet>(pacote.Contents.ToString());

                            if (resposta_registo.success)// Boolean: Se o registo foi feito com sucesso
                            {
                                this.Hide();
                                //Login janela_login = new Login();
                                //janela_login.ShowDialog();
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

        private void button_UserImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg)|*.png;*.jpeg|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFileName = openFileDialog.FileName;
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(selectedFileName);
                bitmap.EndInit();
                ImageBrush_UserImage.ImageSource = bitmap;

                imageBase64 = Convert.ToBase64String(System.IO.File.ReadAllBytes(selectedFileName));
            }
        }
    }
}
