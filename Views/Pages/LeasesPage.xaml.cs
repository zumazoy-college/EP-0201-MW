using EP_0201_MW.Helpers;
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
    /// Логика взаимодействия для LeasesPage.xaml
    /// </summary>
    public partial class LeasesPage : Page
    {
        private User _currentUser;

        public LeasesPage(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;

            if (!RoleHelper.HasAccess(_currentUser, "EditLeases"))
            {
                BtnAddLease.Visibility = Visibility.Collapsed;
                BtnDeleteLease.Visibility = Visibility.Collapsed;
                BtnAddService.Visibility = Visibility.Collapsed;
            }

            RefreshData();
        }

        private void RefreshData()
        {
            using (var db = new MasterSkladDbContext())
            {
                string search = TxtSearchLease.Text.Trim().ToLower();

                var query = db.Leases
                    .Include(l => l.Client)
                    .Include(l => l.Warehouse)
                    .Include(l => l.Pstatus)
                    .Include(l => l.ProvidedServices.Where(ps => !ps.IsDeleted))
                    .Where(l => !l.IsDeleted)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(l => l.ContractNumber.ToLower().Contains(search) ||
                                             l.Client.CompanyName.ToLower().Contains(search));
                }

                DGridLeases.ItemsSource = query.ToList();
            }
        }

        private void TxtSearchLease_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshData();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RoleHelper.CheckAccess(_currentUser, "AddLeases");

                LeaseEditWindow win = new LeaseEditWindow(null, _currentUser);
                win.Owner = Window.GetWindow(this);
                if (win.ShowDialog() == true)
                {
                    RefreshData();
                }
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }
        }

        private void DGridLeases_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                RoleHelper.CheckAccess(_currentUser, "EditLeases");

                if (DGridLeases.SelectedItem is Lease selected)
                {
                    LeaseEditWindow win = new LeaseEditWindow(selected, _currentUser);
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

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RoleHelper.CheckAccess(_currentUser, "DeleteLeases");

                if (DGridLeases.SelectedItem is Lease selected)
                {
                    var result = MessageBox.Show($"Вы уверены, что хотите удалить договор №{selected.ContractNumber}?",
                        "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        using (var db = new MasterSkladDbContext())
                        {
                            var lease = db.Leases.Find(selected.IdLease);
                            if (lease != null)
                            {
                                lease.IsDeleted = true;
                                db.SaveChanges();
                                RefreshData();
                            }
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }
        }

        private void BtnAddService_Click(object sender, RoutedEventArgs e)
        {
            if (DGridLeases.SelectedItem is Lease selected)
            {
                // Перезагружаем данные договора с нужными включениями
                using (var db = new MasterSkladDbContext())
                {
                    var leaseWithDetails = db.Leases
                        .Include(l => l.Client)
                        .Include(l => l.Warehouse)
                        .FirstOrDefault(l => l.IdLease == selected.IdLease);

                    if (leaseWithDetails != null)
                    {
                        AdditionalServiceWindow win = new AdditionalServiceWindow(leaseWithDetails);
                        win.Owner = Window.GetWindow(this);
                        if (win.ShowDialog() == true)
                        {
                            // При необходимости можно обновить данные
                            RefreshData();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите договор из таблицы!",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
