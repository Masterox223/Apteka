using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
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
    /// Логика взаимодействия для Pharmacist.xaml
    /// </summary>
    public partial class Pharmacist : Window
    {

        private Dictionary<string, int> medicinesData;

        public Pharmacist()
        {
            InitializeComponent();
            LoadMedicines();
        }

        public class OrderItem
        {
            public string name_user { get; set; }
            public string name_medicines { get; set; }
            public int quantity { get; set; }
            public int cost { get; set; }
            public int total_cost => quantity * cost; // Вычисляемая свойство
            public string status { get; set; }
        }

        private void UpdateQuantity(string medicineName)
        {
            string connectionString = Configuration.ConnectionString;
            if (int.TryParse(quantityTextBox.Text, out int additionalQuantity))
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string selectQuery = "SELECT quantity FROM medicines WHERE name = @name";
                    int currentQuantity = 0;
                    using (var selectCommand = new MySqlCommand(selectQuery, connection))
                    {
                        selectCommand.Parameters.AddWithValue("@name", medicineName);
                        var result = selectCommand.ExecuteScalar();
                        if (result != null && int.TryParse(result.ToString(), out currentQuantity))
                        {
                            int newQuantity = currentQuantity + additionalQuantity;
                            string updateQuery = "UPDATE medicines SET quantity = @quantity WHERE name = @name";
                            using (var updateCommand = new MySqlCommand(updateQuery, connection))
                            {
                                updateCommand.Parameters.AddWithValue("@quantity", newQuantity);
                                updateCommand.Parameters.AddWithValue("@name", medicineName);
                                int rowsAffected = updateCommand.ExecuteNonQuery();
                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("Количество успешно обновлено.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                                else
                                {
                                    MessageBox.Show("Лекарство не найдено.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Лекарство не найдено или не удалось получить текущее количество.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Введите корректное количество.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        public List<OrderItem> GetOrdersForUser()
        {
            List<OrderItem> orders = new List<OrderItem>();
            string connectionString = Configuration.ConnectionString;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT name_user, name_medicines, quantity, cost, status FROM order_client WHERE status='В доставке'";
                MySqlCommand command = new MySqlCommand(query, connection);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string nameUser = reader.GetString("name_user");
                        string nameMedicines = reader.GetString("name_medicines");
                        int quantity = reader.GetInt32("quantity");
                        int cost = reader.GetInt32("cost");
                        string status = reader.GetString("status");

                        orders.Add(new OrderItem
                        {
                            name_user = nameUser,
                            name_medicines = nameMedicines,
                            quantity = quantity,
                            cost = cost,
                            status = status
                        });
                    }
                }
            }

            return orders;
        }

        public List<OrderItem> GetHistoryOrders()
        {
            List<OrderItem> orders = new List<OrderItem>();
            string connectionString = Configuration.ConnectionString;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT name_user, name_medicines, quantity, cost, status FROM order_client WHERE status='Доставлен'";
                MySqlCommand command = new MySqlCommand(query, connection);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string nameUser = reader.GetString("name_user");
                        string nameMedicines = reader.GetString("name_medicines");
                        int quantity = reader.GetInt32("quantity");
                        int cost = reader.GetInt32("cost");
                        string status = reader.GetString("status");

                        orders.Add(new OrderItem
                        {
                            name_user = nameUser,
                            name_medicines = nameMedicines,
                            quantity = quantity,
                            cost = cost,
                            status = status
                        });
                    }
                }
            }

            return orders;
        }

        private void UpdateOrderStatusFromSelectedItem()
        {
            // Проверяем, выбран ли элемент в DataGrid
            if (dataGridOrders.SelectedItem is OrderItem selectedOrder)
            {
                // Получаем данные из выбранного элемента
                string nameUser = selectedOrder.name_user;
                string nameMedicines = selectedOrder.name_medicines;
                int quantity = selectedOrder.quantity;
                int cost = selectedOrder.cost;

                string connectionString = Configuration.ConnectionString;

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Обновляем статус на "Доставлен"
                    string updateQuery = "UPDATE order_client SET status = 'Доставлен' " +
                                         "WHERE name_user = @nameUser AND name_medicines = @nameMedicines " +
                                         "AND quantity = @quantity AND cost = @cost";

                    using (MySqlCommand updateCommand = new MySqlCommand(updateQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@nameUser", nameUser);
                        updateCommand.Parameters.AddWithValue("@nameMedicines", nameMedicines);
                        updateCommand.Parameters.AddWithValue("@quantity", quantity);
                        updateCommand.Parameters.AddWithValue("@cost", cost);

                        updateCommand.ExecuteNonQuery();
                    }

                    MessageBox.Show("Статус успешно обновлен на 'Доставлен'.");
                }
                dataGridOrders.ItemsSource = GetOrdersForUser();
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите элемент для обновления статуса.");
            }
        }

        private void QuantityTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private static bool IsTextAllowed(string text)
        {
            return int.TryParse(text, out _);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (medicineComboBox.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите название лекарства.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(quantityTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите количество.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            UpdateQuantity(medicineComboBox.SelectedItem.ToString());
        }

        private void btnChangeStatus_Click(object sender, RoutedEventArgs e)
        {
            UpdateOrderStatusFromSelectedItem();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dataGridOrders.ItemsSource = GetOrdersForUser();
            historyOrders.ItemsSource = GetHistoryOrders();
        }
    }
}
