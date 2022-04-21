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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cliente
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_entrar_Click(object sender, RoutedEventArgs e)
        {
            // Código temporário para efeitos de teste

            if(!String.IsNullOrEmpty(textBox_nomeUtilizador.Text) || !String.IsNullOrWhiteSpace(textBox_nomeUtilizador.Text))
            {
                this.Hide();
                Chat janela_chat = new Chat(textBox_nomeUtilizador.Text);
                janela_chat.ShowDialog();
            }
            else
            {
                textBox_nomeUtilizador.BorderBrush = Brushes.Red;
            }
        }

        private void button_registar_Click(object sender, RoutedEventArgs e)
        {
            // Código temporário para efeitos de teste

            if (!String.IsNullOrEmpty(textBox_nomeUtilizador.Text) || !String.IsNullOrWhiteSpace(textBox_nomeUtilizador.Text))
            {
                if (!String.IsNullOrEmpty(textBox_palavraPasse.Password) || !String.IsNullOrWhiteSpace(textBox_palavraPasse.Password))
                {
                    this.Hide();
                    Chat janela_chat = new Chat(textBox_nomeUtilizador.Text);
                    janela_chat.ShowDialog();
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

        private void textBlock_linkRegistar_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.Hide();
            registar janela_registar = new registar();
            janela_registar.ShowDialog();
        }

        private void textBlock_linkRegistar_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            textBlock_linkRegistar.TextDecorations = TextDecorations.Underline;
        }

        private void textBlock_linkRegistar_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            TextDecorationCollection decorations = new TextDecorationCollection();
            decorations.Clear();
            textBlock_linkRegistar.TextDecorations = decorations;
        }
    }
}
