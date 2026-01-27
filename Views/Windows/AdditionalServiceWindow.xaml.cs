using EP_0201_MW.Models;
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
using System.Windows.Shapes;

namespace EP_0201_MW.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для AdditionalServiceWindow.xaml
    /// </summary>
    public partial class AdditionalServiceWindow : Window
    {
        private Lease _selectedLease;

        public AdditionalServiceWindow(Lease lease)
        {
            InitializeComponent();
            _selectedLease = lease;
            LoadData();
        }

        private void LoadData()
        {
            using (var db = new MasterSkladDbContext())
            {
                // Загружаем список услуг
                CmbServices.ItemsSource = db.Services
                    .Where(s => !s.IsDeleted)
                    .ToList();

                // Загружаем существующие услуги для этого договора
                DGridProvidedServices.ItemsSource = db.ProvidedServices
                    .Include(ps => ps.Service)
                    .Where(ps => ps.LeaseId == _selectedLease.IdLease && !ps.IsDeleted)
                    .ToList();

                // Отображаем информацию о договоре
                TxtContractNumber.Text = _selectedLease.ContractNumber;
                TxtClient.Text = _selectedLease.Client?.CompanyName;
                TxtWarehouse.Text = _selectedLease.Warehouse?.WarehouseNumber;
            }
        }

        private void BtnAddService_Click(object sender, RoutedEventArgs e)
        {
            if (CmbServices.SelectedItem is Service selectedService &&
                DatePickerServiceDate.SelectedDate.HasValue &&
                int.TryParse(TxtQuantity.Text, out int quantity) && quantity > 0)
            {
                try
                {
                    using (var db = new MasterSkladDbContext())
                    {
                        var providedService = new ProvidedService
                        {
                            ServiceDate = DateOnly.FromDateTime(DatePickerServiceDate.SelectedDate.Value),
                            Quantity = quantity,
                            LeaseId = _selectedLease.IdLease,
                            ServiceId = selectedService.IdService,
                            IsDeleted = false
                        };

                        db.ProvidedServices.Add(providedService);
                        db.SaveChanges();

                        MessageBox.Show("Услуга успешно добавлена!", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        LoadData(); // Обновляем список

                        // Очищаем поля ввода
                        TxtQuantity.Text = "1";
                        DatePickerServiceDate.SelectedDate = DateTime.Now;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении услуги: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, заполните все поля корректно!\n" +
                               "• Выберите услугу\n" +
                               "• Укажите дату\n" +
                               "• Введите количество (целое число > 0)",
                               "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnDeleteService_Click(object sender, RoutedEventArgs e)
        {
            if (DGridProvidedServices.SelectedItem is ProvidedService selected)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить эту услугу?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    using (var db = new MasterSkladDbContext())
                    {
                        var service = db.ProvidedServices.Find(selected.IdProvidedService);
                        if (service != null)
                        {
                            service.IsDeleted = true;
                            db.SaveChanges();
                            LoadData();
                        }
                    }
                }
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
