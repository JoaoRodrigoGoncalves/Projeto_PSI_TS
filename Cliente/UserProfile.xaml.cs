using System;
using System.Windows;

namespace Cliente
{
    public partial class UserProfile : Window
    {
        private string utilizador;
 
        public UserProfile(string username)
        {
            InitializeComponent();

            // Inicialização de variáveis

            utilizador = username;

            // Preencher elementos do UI

            textBlock_nomeUtilizador.Text = username;

            // Dados temporários para preencher o stackpanel
            MessageControl sampleMessage = new MessageControl(username, DateTime.Now, "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec facilisis non diam eu elementum. Nullam.", 260, true);
            MessageControl sampleMessage1 = new MessageControl(username, DateTime.Now, "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec facilisis non diam eu elementum. Nullam.", 260, true);
            MessageControl sampleMessage2 = new MessageControl(username, DateTime.Now, "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec facilisis non diam eu elementum. Nullam.", 260, true);
            messagePanel.Children.Add(sampleMessage);
            messagePanel.Children.Add(sampleMessage1);
            messagePanel.Children.Add(sampleMessage2);
        }
    }
}

