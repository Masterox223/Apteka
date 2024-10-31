using MySql.Data.MySqlClient;
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

namespace Apteka
{
    /// <summary>
    /// Логика взаимодействия для Client.xaml
    /// </summary>
    public partial class Client : Window
    {
        private Dictionary<string, int> medicinesData;

        public string Name { get; set; } // Свойство для хранения имени

        public Client()
        {
            InitializeComponent();
            medicineComboBox.SelectionChanged += MedicineComboBox_SelectionChanged;
            CreateOrderClientTable();
            LoadMedicines();
            LoadQuantities();
        }

        public class OrderItem
        {
            public int Number { get; set; } // Номер
            public string NameMedicines { get; set; } // Имя лекарства
            public int Quantity { get; set; } // Количество
            public int Cost { get; set; } // Стоимость за штуку
            public int TotalCost => Quantity * Cost; // Общая стоимость (вычисляемое свойство)

            public OrderItem(int number, string nameMedicines, int quantity, int cost)
            {
                Number = number;
                NameMedicines = nameMedicines;
                Quantity = quantity;
                Cost = cost;
            }
        }

        public List<OrderItem> GetOrdersForUser(string name)
        {
            string connectionString = Configuration.ConnectionString;

            List<OrderItem> orders = new List<OrderItem>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT id, name_medicines, quantity, cost FROM order_client WHERE name_user = @name AND status = 'В доставке'";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    // Явное указание типа данных для параметра
                    command.Parameters.Add("@name", MySqlDbType.VarChar).Value = name;

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            OrderItem orderItem = new OrderItem(
                                reader.GetInt32("id"), // Номер заказа
                                reader.GetString("name_medicines"), // Имя лекарства
                                reader.GetInt32("quantity"), // Количество
                                reader.GetInt32("cost") // Стоимость за штуку
                            );

                            orders.Add(orderItem);
                        }
                    }
                }
            }

            return orders;
        }

        public List<OrderItem> GetOrdersForUserReceived(string name)
        {
            string connectionString = Configuration.ConnectionString;

            List<OrderItem> orders = new List<OrderItem>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT id, name_medicines, quantity, cost FROM order_client WHERE name_user = @name AND status = 'Доставлен'";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    // Явное указание типа данных для параметра
                    command.Parameters.Add("@name", MySqlDbType.VarChar).Value = name;

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            OrderItem orderItem = new OrderItem(
                                reader.GetInt32("id"), // Номер заказа
                                reader.GetString("name_medicines"), // Имя лекарства
                                reader.GetInt32("quantity"), // Количество
                                reader.GetInt32("cost") // Стоимость за штуку
                            );

                            orders.Add(orderItem);
                        }
                    }
                }
            }

            return orders;
        }

        private void MedicineComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            quantity.SelectedItem = null;
            LoadQuantities();
            if (medicineComboBox.SelectedItem != null)
            {
                string selectedMedicine = medicineComboBox.SelectedItem.ToString();
                int price = medicinesData[selectedMedicine]; // Получаем цену по названию
                priceTextBlock.Text = price.ToString();
            }
        }

        private void UpdateMedicineQuantity()
        {
            string connectionString = Configuration.ConnectionString;

            string selectedMedicineName = medicineComboBox.SelectedItem?.ToString();
            int selectedQuantity = (int)(quantity.SelectedItem ?? 0); // Получаем выбранное количество

            if (string.IsNullOrEmpty(selectedMedicineName) || selectedQuantity <= 0)
            {
                MessageBox.Show("Пожалуйста, выберите лекарство и количество.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string selectQuery = "SELECT quantity FROM medicines WHERE name = @name";
                int currentQuantity = 0;

                using (var selectCommand = new MySqlCommand(selectQuery, connection))
                {
                    selectCommand.Parameters.AddWithValue("@name", selectedMedicineName);
                    var result = selectCommand.ExecuteScalar();

                    if (result != null && int.TryParse(result.ToString(), out currentQuantity))
                    {
                        int newQuantity = currentQuantity - selectedQuantity;
                        string updateQuery = "UPDATE medicines SET quantity = @newQuantity WHERE name = @name";

                        using (var updateCommand = new MySqlCommand(updateQuery, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@newQuantity", newQuantity);
                            updateCommand.Parameters.AddWithValue("@name", selectedMedicineName);
                            updateCommand.ExecuteNonQuery();
                        }

                    }
                    else
                    {
                        MessageBox.Show("Не удалось получить текущее количество лекарства.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void LoadMedicines()
        {
            string connectionString = Configuration.ConnectionString;

            medicinesData = new Dictionary<string, int>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT name, cost FROM medicines";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string name = reader.GetString("name");
                        int price = reader.GetInt32("cost");
                        medicinesData[name] = price; // Сохраняем данные в словарь
                    }
                }
            }

            medicineComboBox.ItemsSource = medicinesData.Keys; // Присваиваем только названия в ComboBox
        }

        private void LoadQuantities()
        {
            string connectionString = Configuration.ConnectionString;

            // Получаем выбранное название лекарства из medicineComboBox
            string selectedMedicineName = medicineComboBox.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(selectedMedicineName))
            {
                quantity.IsEnabled = false; // Отключаем ComboBox, если ничего не выбрано
                return;
            }

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // Запрос для получения quantity по имени лекарства
                string query = "SELECT quantity FROM medicines WHERE name = @name";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", selectedMedicineName);
                    var result = command.ExecuteScalar();

                    if (result != null && int.TryParse(result.ToString(), out int maxQuantity))
                    {
                        if (maxQuantity == 0)
                        {
                            quantity.IsEnabled = false; // Отключаем ComboBox, если quantity равно 0
                        }
                        else
                        {
                            // Создаем список количеств от 1 до maxQuantity
                            List<int> quantities = Enumerable.Range(1, maxQuantity).ToList();
                            quantity.ItemsSource = quantities; // Заполняем ComboBox
                            quantity.IsEnabled = true; // Включаем ComboBox
                        }
                    }
                    else
                    {
                        quantity.IsEnabled = false; // Отключаем ComboBox, если не удалось получить quantity
                    }
                }
            }
        }

        public void CreateOrderClientTable()
        {
            string connectionString = Configuration.ConnectionString;
            string query = @"
            CREATE TABLE IF NOT EXISTS order_client (
                id INT AUTO_INCREMENT PRIMARY KEY,
                name_user VARCHAR(255) NOT NULL,
                name_medicines VARCHAR(255) NOT NULL,
                quantity INT NOT NULL,
                cost INT NOT NULL,
                status VARCHAR(50) NOT NULL
            );";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                        Console.WriteLine("Таблица order_client успешно создана или уже существует.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при создании таблицы: {ex.Message}");
                }
            }
        }
        
        public void AddOrder(string nameUser, string nameMedicines, int quantity, int cost, string status)
        {
            string connectionString = Configuration.ConnectionString;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // Проверяем, существует ли запись с теми же nameUser и nameMedicines
                string checkQuery = "SELECT quantity, status FROM order_client WHERE name_user = @nameUser AND name_medicines = @nameMedicines AND status='В доставке   '";

                using (MySqlCommand checkCommand = new MySqlCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@nameUser", nameUser);
                    checkCommand.Parameters.AddWithValue("@nameMedicines", nameMedicines);

                    using (MySqlDataReader reader = checkCommand.ExecuteReader())
                    {
                        if (reader.Read()) // Если запись найдена
                        {
                            string currentStatus = reader.GetString(1); // Получаем текущий статус
                            MessageBox.Show(currentStatus);

                            if (currentStatus == status) // Проверяем статус
                            {
                                int currentQuantity = reader.GetInt32(0); // Получаем текущее количество
                                int newQuantity = currentQuantity + quantity;

                                // Обновляем количество в существующей записи
                                reader.Close(); // Закрываем DataReader перед выполнением следующего запроса
                                string updateQuery = "UPDATE order_client SET quantity = @newQuantity WHERE name_user = @nameUser AND name_medicines = @nameMedicines";

                                using (MySqlCommand updateCommand = new MySqlCommand(updateQuery, connection))
                                {
                                    updateCommand.Parameters.AddWithValue("@newQuantity", newQuantity);
                                    updateCommand.Parameters.AddWithValue("@nameUser", nameUser);
                                    updateCommand.Parameters.AddWithValue("@nameMedicines", nameMedicines);

                                    updateCommand.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                MessageBox.Show(currentStatus);
                                // Если статус отличается, добавляем новую запись
                                reader.Close(); // Закрываем DataReader перед выполнением следующего запроса
                                string insertQuery = "INSERT INTO order_client (name_user, name_medicines, quantity, cost, status) " +
                                                     "VALUES (@nameUser, @nameMedicines, @quantity, @cost, @status)";

                                using (MySqlCommand insertCommand = new MySqlCommand(insertQuery, connection))
                                {
                                    insertCommand.Parameters.AddWithValue("@nameUser", nameUser);
                                    insertCommand.Parameters.AddWithValue("@nameMedicines", nameMedicines);
                                    insertCommand.Parameters.AddWithValue("@quantity", quantity);
                                    insertCommand.Parameters.AddWithValue("@cost", cost);
                                    insertCommand.Parameters.AddWithValue("@status", status);

                                    insertCommand.ExecuteNonQuery();
                                }
                            }
                        }
                        else // Если запись не найдена, добавляем новую
                        {
                            //MessageBox.Show("");
                            reader.Close(); // Закрываем DataReader перед выполнением следующего запроса
                            string insertQuery = "INSERT INTO order_client (name_user, name_medicines, quantity, cost, status) " +
                                                 "VALUES (@nameUser, @nameMedicines, @quantity, @cost, @status)";

                            using (MySqlCommand insertCommand = new MySqlCommand(insertQuery, connection))
                            {
                                insertCommand.Parameters.AddWithValue("@nameUser", nameUser);
                                insertCommand.Parameters.AddWithValue("@nameMedicines", nameMedicines);
                                insertCommand.Parameters.AddWithValue("@quantity", quantity);
                                insertCommand.Parameters.AddWithValue("@cost", cost);
                                insertCommand.Parameters.AddWithValue("@status", status);

                                insertCommand.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (medicineComboBox.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите название лекарства.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(quantity.Text))
            {
                MessageBox.Show("Пожалуйста, введите количество.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string name = Name;
            string name_medicines = medicineComboBox.SelectedItem.ToString();
            int myQuantity = int.Parse(quantity.SelectedItem.ToString());
            int cost = int.Parse(priceTextBlock.Text);
            string status = "В доставке";
            //string status = "Доставлен";
            AddOrder(name, name_medicines, myQuantity, cost, status);
            UpdateMedicineQuantity();
            LoadQuantities();
            quantity.SelectedItem = null;
            dg.ItemsSource = GetOrdersForUser(Name);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dg.ItemsSource = GetOrdersForUser(Name);
            dg2.ItemsSource = GetOrdersForUserReceived(Name);

        }



    }
}
