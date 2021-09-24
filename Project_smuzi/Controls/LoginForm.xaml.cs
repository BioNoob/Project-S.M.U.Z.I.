using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
        }

        public string Login { get => login; set { login = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Login")); } }
        public string PassWord { get => passWord; set { passWord = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PassWord")); } }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
