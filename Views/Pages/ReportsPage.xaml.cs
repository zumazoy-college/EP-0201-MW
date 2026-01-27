using EP_0201_MW.Helpers;
using EP_0201_MW.Models;
using EP_0201_MW.Views.Windows;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EP_0201_MW.Views.Pages
{
    public partial class ReportsPage : Page
    {
        private User _currentUser;
        private List<string[]> _currentTableData;
        private List<string> _currentColumnHeaders;

        public ReportsPage(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;

            // Проверяем доступ к странице отчетов
            if (!RoleHelper.CanViewReports(_currentUser))
            {
                MessageBox.Show("У вас нет доступа к отчетам", "Доступ запрещен",
                    MessageBoxButton.OK, MessageBoxImage.Warning);

                // Можно перенаправить на главную страницу
                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.MainFrame.Navigate(new MainPage(_currentUser));
                }
                return;
            }

            InitializeReport();
        }

        private void InitializeReport()
        {
            // Устанавливаем текущую дату
            TxtReportDate.Text = DateTime.Now.ToString("dd.MM.yyyy");
            TxtReportPeriod.Text = $"{DateTime.Now:MMMM yyyy}";

            // Устанавливаем ответственного из текущего пользователя
            if (_currentUser?.Employee != null)
            {
                TxtResponsible.Text = $"{_currentUser.Employee.LastName} {_currentUser.Employee.FirstName[0]}.{_currentUser.Employee.MiddleName?[0]}.";
            }
            else
            {
                TxtResponsible.Text = "Иванов А.Н. (по умолчанию)";
            }
        }

        private void ListReports_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListReports.SelectedItem is ListBoxItem selectedItem)
            {
                string reportTag = selectedItem.Tag?.ToString() ?? "";
                BtnGeneratePdf.IsEnabled = !string.IsNullOrEmpty(reportTag);

                // Обновляем заголовок
                string header = selectedItem.Content?.ToString()?.Replace("📊 ", "").Replace("💰 ", "").Replace("📋 ", "") ?? "Отчет";
                TxtReportHeader.Text = header;

                // Загружаем данные отчета
                LoadReportData(reportTag);
            }
        }

        private void LoadReportData(string tag)
        {
            using (var db = new MasterSkladDbContext())
            {
                switch (tag)
                {
                    case "Occupancy":
                        LoadOccupancyReport(db);
                        break;

                    case "Income":
                        LoadIncomeReport(db);
                        break;

                    case "Debts":
                        LoadDebtsReport(db);
                        break;
                }
            }
        }

        private void LoadOccupancyReport(MasterSkladDbContext db)
        {
            // Получаем данные о складах
            var warehouses = db.Warehouses
        .Include(w => w.Status)
        .Include(w => w.Object)
        .Where(w => !w.IsDeleted)
        .ToList();

            // Группируем по статусам с проверкой на null
            var statusGroups = warehouses
                .GroupBy(w => w.Status?.Title ?? "Не указан")
                .Select(g => new
                {
                    Статус = g.Key,
                    Количество = g.Count(),
                    ОбщаяПлощадь = g.Sum(w => w.Area),
                    Процент = warehouses.Count > 0 ?
                        Math.Round((double)g.Count() / warehouses.Count * 100, 1) : 0
                })
                .ToList();

            // Подготавливаем детальные данные для таблицы
            var detailedData = warehouses.Select(w => new
            {
                Номер = w.WarehouseNumber,
                Адрес = w.Object?.Address ?? "Не указан",
                Площадь = $"{w.Area:F2} м²",
                Статус = w.Status?.Title ?? "Не указан",
                Цена = $"{w.MonthlyPrice:N2} ₽",
                Объект = w.Object?.Address ?? "Не указан"
            }).ToList();

            // Устанавливаем данные в таблицу
            DGridReportDetails.ItemsSource = detailedData;

            // Обновляем статистику
            TxtStat1Label.Text = "Всего складов";
            TxtStat1Value.Text = warehouses.Count.ToString();

            TxtStat2Label.Text = "Свободно";
            TxtStat2Value.Text = warehouses.Count(w => w.Status?.Title == "Свободен").ToString();

            TxtStat3Label.Text = "Общая площадь";
            TxtStat3Value.Text = $"{warehouses.Sum(w => w.Area):F2} м²";

            // Обновляем итоговую информацию
            int occupiedCount = warehouses.Count(w => w.Status?.Title == "Занят");
            double occupancyRate = warehouses.Count > 0 ?
                (double)occupiedCount / warehouses.Count * 100 : 0;

            TxtTotalInfo.Text = $"Занятость складов: {occupiedCount} из {warehouses.Count} ({occupancyRate:F1}%)";

            // Обновляем примечание
            TxtNote.Text = "Отчет показывает текущую занятость складских помещений. Статус обновляется автоматически при заключении/окончании договоров аренды.";

            // Сохраняем данные для PDF
            SaveDataForPdf(detailedData);
        }

        private void LoadIncomeReport(MasterSkladDbContext db)
        {
            // Получаем данные о доходах
            var leases = db.Leases
        .Include(l => l.Warehouse)
            .ThenInclude(w => w.Object)
        .Where(l => !l.IsDeleted)
        .ToList(); // Загружаем все данные в память

            var incomeData = leases
                .Where(l => l.Warehouse != null)
                .GroupBy(l => new
                {
                    Объект = l.Warehouse.Object?.Address ?? "Не указан",
                    Год = l.StartDate.Year,
                    Месяц = l.StartDate.Month
                })
                .Select(g => new
                {
                    Объект = g.Key.Объект,
                    Период = $"{g.Key.Год}-{g.Key.Месяц:00}",
                    КоличествоДоговоров = g.Count(),
                    ОбщийДоход = g.Sum(x => x.TotalPrice),
                    СреднийЧек = g.Average(x => x.TotalPrice)
                })
                .OrderByDescending(x => x.Период)
                .ThenBy(x => x.Объект)
                .ToList();

            // Устанавливаем данные в таблицу
            DGridReportDetails.ItemsSource = incomeData.Select(x => new
            {
                Объект = x.Объект,
                Период = x.Период,
                КоличествоДоговоров = x.КоличествоДоговоров,
                ОбщийДоход = $"{x.ОбщийДоход:N2} ₽",
                СреднийЧек = $"{x.СреднийЧек:N2} ₽"
            }).ToList();

            // Расчет общей статистики
            decimal totalIncome = incomeData.Sum(x => x.ОбщийДоход);
            int totalContracts = incomeData.Sum(x => x.КоличествоДоговоров);
            decimal avgIncome = totalContracts > 0 ? totalIncome / totalContracts : 0;

            // Обновляем статистику
            TxtStat1Label.Text = "Общий доход";
            TxtStat1Value.Text = $"{totalIncome:N0} ₽";

            TxtStat2Label.Text = "Договоров";
            TxtStat2Value.Text = totalContracts.ToString();

            TxtStat3Label.Text = "Средний чек";
            TxtStat3Value.Text = $"{avgIncome:N0} ₽";

            // Обновляем итоговую информацию
            TxtTotalInfo.Text = $"Общий доход за период: {totalIncome:N2} ₽ ({totalContracts} договоров)";

            // Обновляем примечание
            TxtNote.Text = "Отчет показывает доходность по объектам. Данные включают только активные договоры аренды (исключены удаленные).";

            // Сохраняем данные для PDF
            var displayData = incomeData.Select(x => new
            {
                Объект = x.Объект,
                Период = x.Период,
                КоличествоДоговоров = x.КоличествоДоговоров,
                ОбщийДоход = $"{x.ОбщийДоход:N2} ₽",
                СреднийЧек = $"{x.СреднийЧек:N2} ₽"
            }).ToList();

            SaveDataForPdf(displayData);
        }

        private void LoadDebtsReport(MasterSkladDbContext db)
        {
            // Получаем данные о задолженностях
            // ID=2 - "Ожидает оплаты" (или другой статус, указывающий на неоплаченный договор)
            var debts = db.Leases
                .Include(l => l.Client)
                .Include(l => l.Pstatus)
                .Include(l => l.Warehouse)
                .Where(l => !l.IsDeleted && l.PstatusId == 2) // Статус "Не оплачен" или "Ожидает оплаты"
                .ToList();

            // Подготавливаем детальные данные
            var detailedData = debts.Select(l =>
            {
                // Правильный расчет дней просрочки
                int daysOverdue = 0;
                var endDate = l.EndDate.ToDateTime(TimeOnly.MinValue);

                // Если дата окончания уже прошла и статус не оплачен - это просрочка
                if (endDate < DateTime.Now && l.PstatusId == 2)
                {
                    daysOverdue = (DateTime.Now - endDate).Days;
                }

                return new
                {
                    Клиент = l.Client?.CompanyName ?? "Не указан",
                    Договор = l.ContractNumber,
                    Склад = l.Warehouse?.WarehouseNumber ?? "Не указан",
                    СуммаДолга = $"{l.TotalPrice:N2} ₽",
                    НачалоАренды = l.StartDate.ToString("dd.MM.yyyy"),
                    Окончание = l.EndDate.ToString("dd.MM.yyyy"),
                    ДнейПросрочки = daysOverdue
                };
            })
            .Where(x => x.ДнейПросрочки > 0) // Показываем только реальные просрочки
            .ToList();

            // Устанавливаем данные в таблицу
            DGridReportDetails.ItemsSource = detailedData;

            // Расчет статистики - только по реальным просрочкам
            var realDebts = debts.Where(l =>
            {
                var endDate = l.EndDate.ToDateTime(TimeOnly.MinValue);
                return endDate < DateTime.Now && l.PstatusId == 2;
            }).ToList();

            decimal totalDebt = realDebts.Sum(l => l.TotalPrice);
            int totalClients = realDebts.Select(l => l.ClientId).Distinct().Count();
            int overdueCount = realDebts.Count;

            // Обновляем статистику
            TxtStat1Label.Text = "Общая задолженность";
            TxtStat1Value.Text = $"{totalDebt:N0} ₽";

            TxtStat2Label.Text = "Клиентов с долгами";
            TxtStat2Value.Text = totalClients.ToString();

            TxtStat3Label.Text = "Просроченных";
            TxtStat3Value.Text = overdueCount.ToString();

            // Обновляем итоговую информацию
            TxtTotalInfo.Text = $"Общая сумма задолженности: {totalDebt:N2} ₽ от {totalClients} клиентов";

            // Обновляем примечание
            TxtNote.Text = "Отчет показывает клиентов с просроченными платежами. Просроченными считаются договоры с истекшим сроком оплаты и статусом 'Ожидает оплаты'.";

            // Сохраняем данные для PDF
            SaveDataForPdf(detailedData);
        }

        private void BtnGeneratePdf_Click(object sender, RoutedEventArgs e)
        {
            // Подготавливаем данные из текущего DataGrid
            PreparePdfDataFromDataGrid();

            if (_currentTableData == null || _currentTableData.Count == 0)
            {
                MessageBox.Show("Нет данных для формирования отчета!",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Подготавливаем статистику для PDF
                var statistics = new Dictionary<string, string>();

                if (!string.IsNullOrEmpty(TxtStat1Value.Text))
                    statistics.Add(TxtStat1Label.Text, TxtStat1Value.Text);

                if (!string.IsNullOrEmpty(TxtStat2Value.Text))
                    statistics.Add(TxtStat2Label.Text, TxtStat2Value.Text);

                if (!string.IsNullOrEmpty(TxtStat3Value.Text))
                    statistics.Add(TxtStat3Label.Text, TxtStat3Value.Text);

                // Генерируем PDF
                string filePath = PdfReportGenerator.GenerateReport(
                    reportTitle: TxtReportHeader.Text,
                    reportDate: TxtReportDate.Text,
                    reportPeriod: TxtReportPeriod.Text,
                    tableData: _currentTableData,
                    columnHeaders: _currentColumnHeaders,
                    statistics: statistics,
                    responsiblePerson: TxtResponsible.Text);

                if (!string.IsNullOrEmpty(filePath))
                {
                    // Открываем PDF файл
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    });

                    MessageBox.Show($"Отчет успешно сохранен:\n{filePath}",
                        "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании PDF: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveDataForPdf<T>(IEnumerable<T> data)
        {
            if (data == null || !data.Any())
            {
                _currentTableData = null;
                _currentColumnHeaders = null;
                return;
            }

            // Получаем имена свойств для заголовков столбцов
            var properties = typeof(T).GetProperties();
            _currentColumnHeaders = properties.Select(p => p.Name).ToList();

            // Преобразуем данные в список массивов строк
            _currentTableData = new List<string[]>();

            foreach (var item in data)
            {
                var row = new List<string>();
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(item)?.ToString() ?? "";
                    row.Add(value);
                }
                _currentTableData.Add(row.ToArray());
            }
        }

        private void PreparePdfDataFromDataGrid()
        {
            // Получаем данные из DataGrid
            var dataGrid = DGridReportDetails;

            if (dataGrid.ItemsSource == null)
            {
                _currentTableData = null;
                _currentColumnHeaders = null;
                return;
            }

            // Получаем заголовки столбцов
            _currentColumnHeaders = new List<string>();
            foreach (var column in dataGrid.Columns)
            {
                if (column.Header != null)
                {
                    _currentColumnHeaders.Add(column.Header.ToString());
                }
            }

            // Получаем данные строк
            _currentTableData = new List<string[]>();
            foreach (var item in dataGrid.Items)
            {
                var row = new List<string>();

                // Для каждого столбца получаем значение ячейки
                foreach (var column in dataGrid.Columns)
                {
                    var cellValue = column.GetCellContent(item);
                    string value = "";

                    if (cellValue is TextBlock textBlock)
                    {
                        value = textBlock.Text;
                    }
                    else if (cellValue is ContentPresenter presenter)
                    {
                        value = presenter.Content?.ToString() ?? "";
                    }

                    row.Add(value);
                }

                _currentTableData.Add(row.ToArray());
            }
        }
    }
}