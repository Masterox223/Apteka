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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfApp;

namespace Apteka
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            CreateMedicinesTable();
            FillMedicinesTable();
            CreateClientTable();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Login login = new Login();
            login.Show();
            this.Hide();

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Password password = new Password();
            password.Show();
            this.Hide();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            CreateAccount createAccount = new CreateAccount();
            createAccount.Show();
            this.Hide();
        }


        public void CreateMedicinesTable()
        {
            string connectionString = Configuration.ConnectionString;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
            CREATE TABLE IF NOT EXISTS medicines (
                id INT AUTO_INCREMENT PRIMARY KEY,
                name VARCHAR(100) NOT NULL,
                cost INT NOT NULL,
                quantity INT
            );";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public void FillMedicinesTable()
        {
            string connectionString = Configuration.ConnectionString;


            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string checkQuery = "SELECT COUNT(*) FROM medicines";
                using (MySqlCommand checkCommand = new MySqlCommand(checkQuery, connection))
                {
                    int count = Convert.ToInt32(checkCommand.ExecuteScalar());
                    if (count > 0)
                    {
                        Console.WriteLine("Таблица уже содержит данные.");
                        return;
                    }
                }

                var medicines = new List<Tuple<string, int, int>>
        {
            Tuple.Create("Аспирин", 50, 0),
            Tuple.Create("Парацетамол", 30, 0),
            Tuple.Create("Ибупрофен", 45, 0),
            Tuple.Create("Но-шпа", 60, 0),
            Tuple.Create("Цитрамон", 40, 0),
            Tuple.Create("Лоперамид", 70, 0),
            Tuple.Create("Эссенциале Форте Н", 800, 0),
            Tuple.Create("Крепкий чай с лимоном", 15, 0),
            Tuple.Create("Календулы настойка", 25, 0),
            Tuple.Create("Декатилен", 150, 0),
            Tuple.Create("Терафлю", 200, 0),
            Tuple.Create("Бифидумбактерин", 100, 0),
            Tuple.Create("Ацикловир", 80, 0),
            Tuple.Create("Супрастин", 50, 0),
            Tuple.Create("Холисал", 120, 0)
        };

                foreach (var medicine in medicines)
                {
                    string name = medicine.Item1;
                    int cost = medicine.Item2;
                    int quantity = medicine.Item3;

                    string query = "INSERT INTO medicines (name, cost, quantity) VALUES (@name, @cost, @quantity)";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@name", name);
                        command.Parameters.AddWithValue("@cost", cost);
                        command.Parameters.AddWithValue("@quantity", quantity);

                        // Выполнение запроса
                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            Console.WriteLine($"Добавлено: {name} - {cost}");
                        }
                        else
                        {
                            Console.WriteLine($"Не удалось добавить: {name}");
                        }
                    }
                }

            }
        }


        public void CreateClientTable()
        {
            string connectionString = Configuration.ConnectionString;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
            CREATE TABLE IF NOT EXISTS client (
                id INT AUTO_INCREMENT PRIMARY KEY,
                name VARCHAR(100) NOT NULL,
                password VARCHAR(255) NOT NULL
            );";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
