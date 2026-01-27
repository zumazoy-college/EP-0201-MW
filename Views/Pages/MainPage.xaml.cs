using EP_0201_MW.Models;
using EP_0201_MW.Views.Windows;
using Microsoft.EntityFrameworkCore;
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
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        private User _currentUser;

        public MainPage(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;

            // Только администратор может редактировать склады
            if (_currentUser?.Role?.Title != "Администратор")
            {
                BtnAdd.Visibility = Visibility.Collapsed;
                BtnDelete.Visibility = Visibility.Collapsed;
            }

            InitializeFilter();
            RefreshData();
        }

        private void InitializeFilter()
        {
            using (var db = new MasterSkladDbContext())
            {
                var statuses = db.WarehouseStatuses.ToList();
                statuses.Insert(0, new WarehouseStatus { IdStatus = 0, Title = "Все статусы" });

                ComboStatus.ItemsSource = statuses;
                ComboStatus.SelectedIndex = 0;
            }
        }

        private void RefreshData()
        {
            using (var db = new MasterSkladDbContext())
            {
                var query = db.Warehouses
                    .Include(w => w.Object)
                    .Include(w => w.Status)
                    .Where(w => !w.IsDeleted)
                    .AsQueryable();

                string search = TxtSearch.Text.Trim().ToLower();
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(w => w.WarehouseNumber.ToLower().Contains(search));
                }

                if (ComboStatus.SelectedItem is WarehouseStatus selectedStatus && selectedStatus.IdStatus != 0)
                {
                    query = query.Where(w => w.StatusId == selectedStatus.IdStatus);
                }

                LViewWarehouses.ItemsSource = query.ToList();
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_currentUser?.Role?.Title != "Администратор")
                {
                    MessageBox.Show("Только администратор может добавлять склады", "Доступ запрещен",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                WarehouseEditWindow win = new WarehouseEditWindow();
                win.Owner = Window.GetWindow(this);
                if (win.ShowDialog() == true)
                {
                    RefreshData();
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        private void LViewWarehouses_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (_currentUser?.Role?.Title != "Администратор")
                {
                    MessageBox.Show("Только администратор может редактировать склады", "Доступ запрещен",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (LViewWarehouses.SelectedItem is Warehouse selected)
                {
                    WarehouseEditWindow win = new WarehouseEditWindow(selected);
                    win.Owner = Window.GetWindow(this);
                    if (win.ShowDialog() == true)
                    {
                        RefreshData();
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_currentUser?.Role?.Title != "Администратор")
                {
                    MessageBox.Show("Только администратор может удалять склады", "Доступ запрещен",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (LViewWarehouses.SelectedItem is Warehouse selected)
                {
                    var result = MessageBox.Show($"Вы действительно хотите удалить склад {selected.WarehouseNumber}?",
                        "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        using (var db = new MasterSkladDbContext())
                        {
                            var warehouse = db.Warehouses.Find(selected.IdWarehouse);
                            if (warehouse != null)
                            {
                                warehouse.IsDeleted = true; // Мягкое удаление
                                db.SaveChanges();
                                RefreshData();
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshData();
        }

        private void ComboStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshData();
        }
    }
}
