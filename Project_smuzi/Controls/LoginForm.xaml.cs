using Project_smuzi.Classes;
using Project_smuzi.Models;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace Project_smuzi.Controls
{
    /// <summary>
    /// Логика взаимодействия для LoginForm.xaml
    /// </summary>
    public partial class LoginForm : Window, INotifyPropertyChanged
    {
        private string login;

        private string passWord;

        public LoginForm()
        {
            InitializeComponent();
            //UserControl w = new UserControl();
            //w.Show();
            SharedModel.DB_Workers = NpcBase.LoadFromJson();
            SharedModel.CloseEvent += SharedModel_CloseEvent;
        }

        private void SharedModel_CloseEvent()
        {
            this.Close();
        }

        public string Login { get => login; set { login = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Login")); } }
        public string PassWord { get => passWord; set { passWord = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PassWord")); } }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var a = SharedModel.DB_Workers.Workers.Where(t => t.Name == Login).ToList();
            //login ok
            if (a.Count > 0)
            {
                var s = a.FirstOrDefault(t => t.Password == PassWord);
                //pass ok
                if (s != null)
                {
                    SharedModel.CurrentUser = s;
                    if (s.IsAdmin)
                    {
                        UserControl uc = new UserControl();
                        ScladControl sc = new ScladControl();
                        MainWindow mw = new MainWindow();
                        uc.Show();
                        sc.Show();
                        mw.Show();
                        this.Hide();
                    }
                    else
                    {
                        if (s.WorkerGroups.FirstOrDefault(t => t.SectorLabel.Contains("Склад")) != null)
                        {
                            ScladControl sc = new ScladControl();
                            sc.Show();
                            this.Hide();
                        }
                        else
                        {
                            MainWindow mw = new MainWindow();
                            mw.Show();
                            this.Hide();
                        }
                    }
                }
                else
                    System.Windows.Forms.MessageBox.Show("Пароль не верен!");
            }
            else
                System.Windows.Forms.MessageBox.Show("Логин не найден!");
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(PassBx.Password))
                PassWord = PassBx.Password;
        }
    }
}
