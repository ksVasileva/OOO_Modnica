using EasyCaptcha.Wpf;
using OOO_Modnica.Classes;
using OOO_Modnica.Model;
using OOO_Modnica.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace OOO_Modnica
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Classes.Helper.DBHall = new Model.EntitiesDB();
            MyCaptcha.CreateCaptcha(Captcha.LetterOption.Alphanumeric, 5);
            
        }

        private DispatcherTimer captchaTimer;
        private int remainingSeconds = 10;

        private void StartCaptchaTimer()
        {
            captchaTimer = new DispatcherTimer();
            captchaTimer.Interval = TimeSpan.FromSeconds(1); // таймер срабатывает каждую секунду
            captchaTimer.Tick += CaptchaTimer_Tick;
            remainingSeconds = 10; // начинаем с 10 секунд
            captchaTimer.Start();
        }

        private void CaptchaTimer_Tick(object sender, EventArgs e)
        {
            remainingSeconds--;

            if (remainingSeconds <= 0)
            {
                captchaTimer.Stop();
                captchaTimer = null;
                buttonInput.IsEnabled = true;
            }

            buttonInput.Content = "Войти" +  ((remainingSeconds > 0) ? " (" + remainingSeconds.ToString() + ")" : "");
            this.InvalidateVisual();
        }

        private void buttonNavigate_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonInput_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonInput_Click_1(object sender, RoutedEventArgs e)
        {
            String check_login = textBoxLogin.Text;
            String check_password = textBlockPassword.Text;

            StringBuilder sb = new StringBuilder();

            if (String.IsNullOrEmpty(check_login) || String.IsNullOrEmpty(check_password))
            {
                sb.AppendLine("Неверный логин или пароль");
            }
            if(captcha.Text != MyCaptcha.CaptchaText)
            {
                sb.AppendLine("Вы робот. Введите каптчу заного!");
                buttonInput.IsEnabled = false;
                StartCaptchaTimer();
                MyCaptcha.CreateCaptcha(Captcha.LetterOption.Alphanumeric, 5);
            }
            if (sb.Length > 0)
            {
                MessageBox.Show(sb.ToString());
                return;
            }
            //получение всех пользователей
            List<Model.User> users = new List<Model.User>();
            users = Helper.DBHall.User.ToList();

            Model.User user = Helper.DBHall.User.Where(u => u.UserLogin == check_login && u.UserPassword == check_password).FirstOrDefault();
            if (user != null)
            {
                MessageBox.Show("Добро пожаловать, " + user.UserFullName + " " + user.Role.RoleName);

                goCatalog(user);
            }
            else
            {                                                                                                    
                MessageBox.Show("Возникла небольшая ошибка: Пользователи с такими данными не были найдены!! Попробуйте ввести логин и пароль еще раз, будьте внимательнее:)");
            }
        }

        private void buttonGuest_Click(object sender, RoutedEventArgs e)
        {
            goCatalog();
           
        }

        private void textBoxLogin_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        public void goCatalog() 
        {
            Catalog catalog = new Catalog();
            this.Hide();
            catalog.ShowDialog();
            this.Show();
        }

        public void goCatalog(User user)
        {
            Catalog catalog = new Catalog(user);
            this.Hide();
            catalog.ShowDialog();
            this.Show();
        }

        private void textBlockPassword_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
