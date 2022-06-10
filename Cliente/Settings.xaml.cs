using System.Windows;

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

            if (TB_IP.Text != Properties.Settings.Default.ipaddress)
            {
                Properties.Settings.Default.ipaddress = TB_IP.Text;
                changes = true;
            }

            if (TB_Port.Text != Properties.Settings.Default.port.ToString())
            {
                Properties.Settings.Default.port = int.Parse(TB_Port.Text);
                changes = true;
            }

            if (changes)
            {
                Properties.Settings.Default.Save();
                Close();
            }
        }
    }
}
