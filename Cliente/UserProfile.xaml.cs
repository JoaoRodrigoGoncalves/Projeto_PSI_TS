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
        }
    }
}

