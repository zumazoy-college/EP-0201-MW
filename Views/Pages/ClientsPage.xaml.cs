using EP_0201_MW.Helpers;
using EP_0201_MW.Models;
using EP_0201_MW.Views.Windows;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EP_0201_MW.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для ClientsPage.xaml
    /// </summary>
    public partial class ClientsPage : Page
    {
        private User _currentUser;

        public ClientsPage(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;

            // Проверяем доступ
            if (!RoleHelper.HasAccess(_currentUser, "EditClients"))
            {
                BtnAddClient.Visibility = Visibility.Collapsed;
                BtnDeleteClient.Visibility = Visibility.Collapsed;
            }

            RefreshData();
        }

        private void RefreshData()
        {
            using (var db = new MasterSkladDbContext())
            {
                string search = TxtSearchClient.Text.Trim().ToLower();

                // Берем только тех, у кого isDeleted == false
                var query = db.Clients.Where(c => !c.IsDeleted).AsQueryable();

                // Фильтрация по названию компании или фамилии
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(c => c.CompanyName.ToLower().Contains(search) ||
                                                         c.LastNamePerson.ToLower().Contains(search));
                }

                DGridClients.ItemsSource = query.ToList();
            }
        }

        private void TxtSearchClient_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshData();
        }

        private void BtnAddClient_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RoleHelper.CheckAccess(_currentUser, "AddClients");

                ClientEditWindow win = new ClientEditWindow();
                win.Owner = Window.GetWindow(this);
                if (win.ShowDialog() == true)
                {
                    RefreshData();
                }
            }
            catch (UnauthorizedAccessException)
            {
                return; // Сообщение уже показано в CheckAccess
            }
        }

        private void DGridClients_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                RoleHelper.CheckAccess(_currentUser, "EditClients");

                if (DGridClients.SelectedItem is Client selected)
                {
                    ClientEditWindow win = new ClientEditWindow(selected);
                    win.Owner = Window.GetWindow(this);
                    if (win.ShowDialog() == true)
                    {
                        RefreshData();
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }
        }

        private void BtnDeleteClient_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RoleHelper.CheckAccess(_currentUser, "DeleteClients");

                if (DGridClients.SelectedItem is Client selected)
                {
                    var result = MessageBox.Show($"Вы уверены, что хотите удалить клиента \"{selected.CompanyName}\"?",
                        "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        using (var db = new MasterSkladDbContext())
                        {
                            var client = db.Clients.Find(selected.IdClient);
                            if (client != null)
                            {
                                client.IsDeleted = true; // Мягкое удаление
                                db.SaveChanges();
                                RefreshData();
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Пожалуйста, выберите клиента из списка.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }            
        }
    }
}
