﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Cliente
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            TB_IP.Text = Properties.Settings.Default.ipaddress;
            TB_Port.Text = Properties.Settings.Default.port.ToString();
        }

        private void BTN_Cancelar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BTN_OK_Click(object sender, RoutedEventArgs e)
        {
            bool changes = false;

            if(TB_IP.Text != Properties.Settings.Default.ipaddress)
            {
                Properties.Settings.Default.ipaddress = TB_IP.Text;
                changes = true;
            }

            if(TB_Port.Text != Properties.Settings.Default.port.ToString())
            {
                Properties.Settings.Default.port = int.Parse(TB_Port.Text);
                changes = true;
            }

            if(changes)
            {
                MessageBox.Show("É necessário reiniciar a aplicação para aplicar as alterações.", "Reinicio necessário", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                Properties.Settings.Default.Save();
                Session.FecharSessao();
                Process.Start(Application.ResourceAssembly.Location); // Inicia uma nova instancia da aplicação
                Environment.Exit(0); // Termina esta instancia da aplicação
            }
        }
    }
}
