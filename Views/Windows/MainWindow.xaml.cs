using EP_0201_MW.Models;
using EP_0201_MW.Views.Pages;
using System;
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

namespace EP_0201_MW.Views.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private User _currentUser;
        private Button _activeMenuButton;

        public MainWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;

            InitializeMenuBasedOnRole();

            // Выводим ФИО сотрудника в верхнюю панель
            string fio = $"{_currentUser.Employee.LastName} {_currentUser.Employee.FirstName[0]}.";
            if (!string.IsNullOrEmpty(_currentUser.Employee.MiddleName))
                fio += $"{_currentUser.Employee.MiddleName[0]}.";
            TxtUserFio.Text = fio;

            // Выводим роль пользователя
            TxtUserRole.Text = _currentUser.Role?.Title ?? "Неизвестная роль";

            // Устанавливаем активную кнопку "Главная"
            SetActiveMenuButton(BtnMain);

            MainFrame.Navigate(new MainPage(_currentUser));
        }

        public MainWindow() : this(null!) { }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                // Обновляем заголовок страницы
                string content = btn.Content.ToString();
                if (content.Length > 2)
                {
                    TxtPageTitle.Text = content.Substring(2);
                }

                string target = btn.Tag.ToString();

                // Переключаем страницы
                switch (target)
                {
                    case "Main":
                        MainFrame.Navigate(new MainPage(_currentUser));
                        break;
                    case "Clients":
                        MainFrame.Navigate(new ClientsPage(_currentUser));
                        break;
                    case "Leases":
                        MainFrame.Navigate(new LeasesPage(_currentUser));
                        break;
                    case "Reports":
                        MainFrame.Navigate(new ReportsPage(_currentUser));
                        break;
                }

                // Обновляем активную кнопку меню
                SetActiveMenuButton(btn);
            }
        }

        private void SetActiveMenuButton(Button activeButton)
        {
            // Сбрасываем стиль предыдущей активной кнопки
            if (_activeMenuButton != null)
            {
                _activeMenuButton.Style = (Style)FindResource("MenuButtonStyle");
            }

            // Устанавливаем стиль для новой активной кнопки
            activeButton.Style = (Style)FindResource("ActiveMenuButtonStyle");
            _activeMenuButton = activeButton;
        }

        private void BtnHelp_Click(object sender, RoutedEventArgs e)
        {
            string helpMessage = "📋 **Справка по работе с программой**\n\n" +
                               "**Основные функции:**\n" +
                               "• Для редактирования данных нажмите на строку в таблице ДВАЖДЫ\n" +
                               "• Для добавления новой записи используйте кнопку '➕ Добавить'\n" +
                               "• Для удаления записи выделите строку и нажмите '🗑 Удалить'\n\n" +
                               "**Быстрые клавиши:**\n" +
                               "• **F1** - открыть справку\n" +
                               "**Советы:**\n" +
                               "• Все изменения сохраняются автоматически\n" +
                               "• Для формирования отчетов используйте вкладку '📊 Отчёты'\n" +
                               "• Для выхода из программы нажмите '🚪 Выход'";

            MessageBox.Show(helpMessage,
                          "❓ Помощь",
                          MessageBoxButton.OK,
                          MessageBoxImage.Information);
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти из системы?\n" +
                                       "Все несохраненные данные будут потеряны.",
                                       "Подтверждение выхода",
                                       MessageBoxButton.YesNo,
                                       MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Открываем окно авторизации
                var loginWindow = new LoginWindow();
                loginWindow.Show();

                // Закрываем главное окно
                this.Close();
            }
        }

        private void InitializeMenuBasedOnRole()
        {
            if (_currentUser?.Role == null) return;

            string role = _currentUser.Role.Title;

            // Менеджер: Reports отключены
            if (role == "Менеджер")
            {
                BtnMain.IsEnabled = true;
                BtnClients.IsEnabled = true;
                BtnLeases.IsEnabled = true;
                BtnReports.IsEnabled = false;
                BtnReports.Visibility = Visibility.Collapsed; // Скрываем полностью
            }
        }


        // Обработка горячих клавиш
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            switch (e.Key)
            {
                case Key.F1:
                    BtnHelp_Click(null, null);
                    e.Handled = true;
                    break;
            }
        }

        // Предотвращаем изменение размера окна меньше минимального
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (this.Width < 1000)
                this.Width = 1000;

            if (this.Height < 600)
                this.Height = 600;
        }
    }
}