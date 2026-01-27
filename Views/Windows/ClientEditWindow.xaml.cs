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
    /// Логика взаимодействия для ClientEditWindow.xaml
    /// </summary>
    public partial class ClientEditWindow : Window
    {
        private Client _currentClient;

        public ClientEditWindow(Client? selectedClient = null)
        {
            InitializeComponent();

            if (selectedClient != null)
            {
                _currentClient = selectedClient;
                this.Title = "Редактирование клиента";
                TxtCompany.Text = _currentClient.CompanyName;
                TxtLastName.Text = _currentClient.LastNamePerson;
                TxtFirstName.Text = _currentClient.FirstNamePerson;
                TxtMiddleName.Text = _currentClient.MiddleNamePerson;
                TxtPhone.Text = _currentClient.PhoneNumber;
                TxtEmail.Text = _currentClient.Email;
            }
            else
            {
                _currentClient = new Client();
                this.Title = "Новый клиент";
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Простейшая валидация
            if (string.IsNullOrWhiteSpace(TxtCompany.Text) || string.IsNullOrWhiteSpace(TxtPhone.Text))
            {
                MessageBox.Show("Заполните название компании и телефон!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new MasterSkladDbContext())
                {
                    // Обновляем объект данными из полей
                    _currentClient.CompanyName = TxtCompany.Text;
                    _currentClient.LastNamePerson = TxtLastName.Text;
                    _currentClient.FirstNamePerson = TxtFirstName.Text;
                    _currentClient.MiddleNamePerson = TxtMiddleName.Text;
                    _currentClient.PhoneNumber = TxtPhone.Text;
                    _currentClient.Email = TxtEmail.Text;

                    if (_currentClient.IdClient == 0) // Добавление
                    {
                        // Проверка уникальности номера телефона (как пример ошибки БД)
                        if (db.Clients.Any(c => c.PhoneNumber == _currentClient.PhoneNumber))
                            throw new Exception("Клиент с таким номером телефона уже зарегистрирован в базе.");

                        db.Clients.Add(_currentClient);
                    }
                    else // Редактирование
                    {
                        db.Clients.Update(_currentClient);
                    }

                    db.SaveChanges();
                    DialogResult = true; // Сигнализируем об успехе
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
    }
}
