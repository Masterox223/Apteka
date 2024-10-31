using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
using MySql.Data;
using MySql.Data.MySqlClient;

namespace Apteka
{
    /// <summary>
    /// Логика взаимодействия для CreateAccount.xaml
    /// </summary>
    public partial class CreateAccount : Window
    {
        public CreateAccount()
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
            if (passwordUser.Length < 6)
            {
                MessageBox.Show("Пароль должен быть минимум 6 символа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            AddUser(nameUser, passwordUser);
            //MessageBox.Show("такой аккаунт есть");
            Client client = new Client();
            client.Name = nameUser;
            client.Show();
            this.Hide();
        }



        public void AddUser(string name, string password)
        {
            string connectionString = Configuration.ConnectionString;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = "INSERT INTO client (name, password) VALUES (@name, @password)";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@password", password);
                    command.ExecuteNonQuery();
                }
            }
        }

    }
}
