using System;
using System.Collections.Generic;
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
    /// Interaction logic for registar.xaml
    /// </summary>
    public partial class registar : Window
    {
        public registar()
        {
            InitializeComponent();
        }

        private void button_registar_Click(object sender, RoutedEventArgs e)
        {
            // Código temporário para efeitos de teste

            if (!String.IsNullOrEmpty(textBox_nomeUtilizador.Text) || !String.IsNullOrWhiteSpace(textBox_nomeUtilizador.Text))
            {
                if (!String.IsNullOrEmpty(textBox_palavraPasse.Password) || !String.IsNullOrWhiteSpace(textBox_palavraPasse.Password))
                {
                    if (!String.IsNullOrEmpty(textBox_verificarPalavraPasse.Password) || !String.IsNullOrWhiteSpace(textBox_verificarPalavraPasse.Password))
                    {
                        if (textBox_palavraPasse.Password == textBox_verificarPalavraPasse.Password) {
                            this.Hide();
                            MainWindow janela_login = new MainWindow();
                            janela_login.ShowDialog();
                        }
                    }
                    else
                    {
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
