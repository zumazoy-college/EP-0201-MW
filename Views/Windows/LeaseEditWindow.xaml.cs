using EP_0201_MW.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EP_0201_MW.Views.Windows
{
    public partial class LeaseEditWindow : Window
    {
        private Lease _currentLease;
        private decimal _monthlyPrice = 0;
        private User _currentUser;

        public LeaseEditWindow(Lease? selectedLease = null, User currentUser = null)
        {
            InitializeComponent();
            _currentUser = currentUser;
            LoadCombos();

            if (selectedLease != null)
            {
                _currentLease = selectedLease;
                this.Title = "Редактирование договора";
                TxtNumber.Text = _currentLease.ContractNumber;
                TxtPrice.Text = _currentLease.TotalPrice.ToString("N2");
                DpStart.SelectedDate = _currentLease.StartDate.ToDateTime(TimeOnly.MinValue);
                DpEnd.SelectedDate = _currentLease.EndDate.ToDateTime(TimeOnly.MinValue);

                // Устанавливаем значения через SelectedValue
                ComboClient.SelectedValue = _currentLease.ClientId;
                ComboWarehouse.SelectedValue = _currentLease.WarehouseId;
                ComboPStatus.SelectedValue = _currentLease.PstatusId;

                // Получаем месячную стоимость выбранного склада
                if (ComboWarehouse.SelectedItem is Warehouse selectedWarehouse)
                {
                    _monthlyPrice = selectedWarehouse.MonthlyPrice;
                }
            }
            else
            {
                _currentLease = new Lease
                {
                    StartDate = DateOnly.FromDateTime(DateTime.Now),
                    EndDate = DateOnly.FromDateTime(DateTime.Now).AddMonths(1)
                };
                DpStart.SelectedDate = DateTime.Now;
                DpEnd.SelectedDate = DateTime.Now.AddMonths(1);
            }

            // Подписываемся на события
            DpStart.SelectedDateChanged += DatePicker_SelectedDateChanged;
            DpEnd.SelectedDateChanged += DatePicker_SelectedDateChanged;
        }

        private void LoadCombos()
        {
            using (var db = new MasterSkladDbContext())
            {
                // Загружаем клиентов
                var clients = db.Clients.Where(c => !c.IsDeleted).ToList();
                ComboClient.ItemsSource = clients;
                ComboClient.SelectedValuePath = "IdClient";

                // Загружаем склады
                var warehouses = db.Warehouses
                    .Include(w => w.Object)
                    .Where(w => !w.IsDeleted)
                    .ToList();
                ComboWarehouse.ItemsSource = warehouses;
                ComboWarehouse.SelectedValuePath = "IdWarehouse";

                // Загружаем статусы оплаты
                var statuses = db.PaymentStatuses.ToList();
                ComboPStatus.ItemsSource = statuses;
                ComboPStatus.SelectedValuePath = "IdPstatus";
            }
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            CalculateTotalPrice();
        }

        private void ComboWarehouse_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboWarehouse.SelectedItem is Warehouse selectedWarehouse)
            {
                _monthlyPrice = selectedWarehouse.MonthlyPrice;
                CalculateTotalPrice();
            }
        }

        private void CalculateTotalPrice()
        {
            if (DpStart.SelectedDate.HasValue &&
                DpEnd.SelectedDate.HasValue &&
                _monthlyPrice > 0)
            {
                var startDate = DpStart.SelectedDate.Value;
                var endDate = DpEnd.SelectedDate.Value;

                // Проверка, что дата окончания не раньше даты начала
                if (endDate < startDate)
                {
                    MessageBox.Show("Дата окончания не может быть раньше даты начала!",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    DpEnd.SelectedDate = startDate.AddMonths(1);
                    return;
                }

                // Расчет периода в днях
                var totalDays = (endDate - startDate).TotalDays;

                // Если период меньше 1 дня
                if (totalDays <= 0)
                {
                    TxtPrice.Text = "0";
                    return;
                }

                // Расчет стоимости
                decimal calculatedPrice;

                // Если аренда меньше месяца - берем полную месячную стоимость
                if (totalDays <= 30)
                {
                    calculatedPrice = _monthlyPrice;
                }
                else
                {
                    // Расчет полных месяцев и остатка дней
                    int fullMonths = (int)(totalDays / 30);
                    int remainingDays = (int)(totalDays % 30);

                    calculatedPrice = _monthlyPrice * fullMonths;

                    // Расчет стоимости за остаток дней (пропорционально)
                    if (remainingDays > 0)
                    {
                        decimal dailyPrice = _monthlyPrice / 30;
                        calculatedPrice += dailyPrice * remainingDays;
                    }
                }

                // Округляем до 2 знаков
                calculatedPrice = Math.Round(calculatedPrice, 2);
                TxtPrice.Text = calculatedPrice.ToString("N2");
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(TxtNumber.Text))
            {
                MessageBox.Show("Введите номер договора!", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ComboClient.SelectedValue == null)
            {
                MessageBox.Show("Выберите клиента!", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ComboWarehouse.SelectedValue == null)
            {
                MessageBox.Show("Выберите склад!", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!DpStart.SelectedDate.HasValue || !DpEnd.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите даты начала и окончания!", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(TxtPrice.Text, out decimal totalPrice) || totalPrice <= 0)
            {
                MessageBox.Show("Введите корректную сумму договора!", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ComboPStatus.SelectedValue == null)
            {
                MessageBox.Show("Выберите статус оплаты!", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new MasterSkladDbContext())
                {
                    // Получаем значения из формы
                    string contractNumber = TxtNumber.Text.Trim();
                    int clientId = (int)ComboClient.SelectedValue;
                    int warehouseId = (int)ComboWarehouse.SelectedValue;
                    int pstatusId = (int)ComboPStatus.SelectedValue;

                    // Определяем менеджера
                    int managerId;
                    if (_currentUser != null && _currentUser.EmployeeId > 0)
                    {
                        managerId = _currentUser.EmployeeId;
                    }
                    else
                    {
                        var defaultManager = db.Employees.FirstOrDefault();
                        if (defaultManager != null)
                        {
                            managerId = defaultManager.IdEmployee;
                        }
                        else
                        {
                            MessageBox.Show("Не удалось определить менеджера договора!",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }

                    if (_currentLease.IdLease == 0)
                    {
                        // Создание нового договора
                        Lease newLease = new Lease
                        {
                            ContractNumber = contractNumber,
                            TotalPrice = totalPrice,
                            StartDate = DateOnly.FromDateTime(DpStart.SelectedDate.Value),
                            EndDate = DateOnly.FromDateTime(DpEnd.SelectedDate.Value),
                            ClientId = clientId,
                            WarehouseId = warehouseId,
                            ManagerId = managerId,
                            PstatusId = pstatusId,
                            IsDeleted = false
                        };
                        db.Leases.Add(newLease);
                    }
                    else
                    {
                        // Редактирование существующего договора
                        var existing = db.Leases.Find(_currentLease.IdLease);
                        if (existing != null)
                        {
                            existing.ContractNumber = contractNumber;
                            existing.TotalPrice = totalPrice;
                            existing.StartDate = DateOnly.FromDateTime(DpStart.SelectedDate.Value);
                            existing.EndDate = DateOnly.FromDateTime(DpEnd.SelectedDate.Value);
                            existing.ClientId = clientId;
                            existing.WarehouseId = warehouseId;
                            existing.ManagerId = managerId;
                            existing.PstatusId = pstatusId;
                        }
                    }

                    db.SaveChanges();
                    DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // Перегружаем конструкторы
        public LeaseEditWindow() : this(null, null) { }
        public LeaseEditWindow(Lease selectedLease) : this(selectedLease, null) { }
    }
}