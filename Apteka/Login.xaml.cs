using MySql.Data.MySqlClient;
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
using WpfApp;

namespace Apteka
{
    /// <summary>
    /// Логика взаимодействия для Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }






        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string nameUser = name.Text;
            string passwordUser = password.Password;

            if (nameUser.Length < 4)
            {
                MessageBox.Show("Логин должен быть минимум 4 символа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (passwordUser.Length < 4)
            {
                MessageBox.Show("Пароль д6олжен быть минимум 6 символа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!CheckUserName(nameUser))
            {
                MessageBox.Show("Такого аккаунта не существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!CheckUserExists(nameUser, passwordUser))
            {
                MessageBox.Show("Неправильный логин или пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            //MessageBox.Show("аккаунт создан все ок");
            Client client = new Client();
            client.Name = nameUser;
            client.Show();
            this.Hide();
        }

        public bool CheckUserExists(string name, string password)
        {
            string connectionString = Configuration.ConnectionString;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT COUNT(*) FROM client WHERE name = @name AND password = @password";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@password", password);

                    int userCount = Convert.ToInt32(command.ExecuteScalar());
                    return userCount > 0;
                }
            }
        }

        public bool CheckUserName(string name)
        {
            string connectionString = Configuration.ConnectionString;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT COUNT(*) FROM client WHERE name = @name";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);

                    int userCount = Convert.ToInt32(command.ExecuteScalar());
                    return userCount > 0;
                }
            }
        }
    }
}
