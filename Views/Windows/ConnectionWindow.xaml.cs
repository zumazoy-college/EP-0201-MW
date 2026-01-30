using EP_0201_MW.Helpers;
using EP_0201_MW.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EP_0201_MW.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для ConnectionWindow.xaml
    /// </summary>
    public partial class ConnectionWindow : Window
    {
        public ConnectionWindow()
        {
            InitializeComponent();
            TxtConnectionString.Text = ConnectionManager.GetConnectionString();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string newConn = TxtConnectionString.Text;
            ConnectionManager.SaveConnectionString(newConn);

            try
            {
                using (var db = new MasterSkladDbContext())
                {
                    if (db.Database.CanConnect())
                    {
                        MessageBox.Show("Успешно!", "Проверка", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.DialogResult = true;
                    }
                    else
                    {
                        MessageBox.Show("Файл сохранен, но подключиться не удалось. Проверьте имя сервера.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}
