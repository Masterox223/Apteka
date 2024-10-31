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

namespace Apteka
{
    /// <summary>
    /// Логика взаимодействия для Password.xaml
    /// </summary>
    public partial class Password : Window
    {
        public Password()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string myPassword = "1234";
            if (myPassword == text.Text)
            {
                Pharmacist pharmacist = new Pharmacist();
                pharmacist.Show();
                this.Hide();
            } else
            {
                MessageBox.Show("Неправильный пароль для входа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
