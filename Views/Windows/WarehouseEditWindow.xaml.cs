using EP_0201_MW.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text;
using System.Windows;

namespace EP_0201_MW.Views.Windows
{
    public partial class WarehouseEditWindow : Window
    {
        private Warehouse _currentWarehouse;

        public WarehouseEditWindow(Warehouse? selectedWarehouse = null)
        {
            InitializeComponent();
            LoadData(); // Сначала грузим списки

            if (selectedWarehouse != null)
            {
                // Для редактирования: работаем с копией или тем же объектом
                _currentWarehouse = selectedWarehouse;
                this.Title = "Редактирование склада";

                // Заполняем текстовые поля
                TxtNumber.Text = _currentWarehouse.WarehouseNumber;
                TxtArea.Text = _currentWarehouse.Area.ToString();
                TxtPrice.Text = _currentWarehouse.MonthlyPrice.ToString();

                // Устанавливаем выбранные элементы в ComboBox через их ID
                ComboObject.SelectedValue = _currentWarehouse.ObjectId;
                ComboStatus.SelectedValue = _currentWarehouse.StatusId;
            }
            else
            {
                _currentWarehouse = new Warehouse();
                this.Title = "Новый склад";
            }
        }

        private void LoadData()
        {
            using (var db = new MasterSkladDbContext())
            {
                // Привязываем списки и указываем, какое свойство является ключом (ID)
                var objects = db.Objects.ToList();
                ComboObject.ItemsSource = objects;
                ComboObject.SelectedValuePath = "IdObject"; // Важно!

                var statuses = db.WarehouseStatuses.ToList();
                ComboStatus.ItemsSource = statuses;
                ComboStatus.SelectedValuePath = "IdStatus"; // Важно!
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // 1. Валидация
            if (string.IsNullOrWhiteSpace(TxtNumber.Text) ||
                ComboObject.SelectedValue == null ||
                ComboStatus.SelectedValue == null)
            {
                MessageBox.Show("Заполните все поля!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new MasterSkladDbContext())
                {
                    // Получаем значения из полей
                    string number = TxtNumber.Text;
                    decimal area = decimal.Parse(TxtArea.Text);
                    decimal price = decimal.Parse(TxtPrice.Text);
                    int objectId = (int)ComboObject.SelectedValue;
                    int statusId = (int)ComboStatus.SelectedValue;

                    if (_currentWarehouse.IdWarehouse == 0)
                    {
                        // Создание нового
                        Warehouse newWarehouse = new Warehouse
                        {
                            WarehouseNumber = number,
                            Area = area,
                            MonthlyPrice = price,
                            ObjectId = objectId,
                            StatusId = statusId,
                            IsDeleted = false
                        };
                        db.Warehouses.Add(newWarehouse);
                    }
                    else
                    {
                        // Редактирование существующего
                        var existing = db.Warehouses.Find(_currentWarehouse.IdWarehouse);
                        if (existing != null)
                        {
                            existing.WarehouseNumber = number;
                            existing.Area = area;
                            existing.MonthlyPrice = price;
                            existing.ObjectId = objectId;
                            existing.StatusId = statusId;
                        }
                    }

                    db.SaveChanges();
                    DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}