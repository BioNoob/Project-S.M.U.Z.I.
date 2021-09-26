using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Project_smuzi.Controls
{
    /// <summary>
    /// Логика взаимодействия для NewUserControl.xaml
    /// </summary>
    public partial class NewUserControl : Window, INotifyPropertyChanged
    {
        private bool mode;
        private bool isAdm;
        private string btntext;
        private string fIO;
        private string pass;

        public NewUserControl()
        {
            InitializeComponent();
            this.IsVisibleChanged += NewUserControl_IsVisibleChanged;
        }

        private void NewUserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (mode)
                ButtonText = "Изменить";
            else
                ButtonText = "Добавить";
        }

        public string FIO { get => fIO; set { SetProperty(ref fIO, value);if (string.IsNullOrWhiteSpace(value)) Btn_enabled = false; else Btn_enabled = true; } }
        public string Password { get => pass; set { SetProperty(ref pass, value); } }

        private bool btn_enabled;
        public bool Btn_enabled { get => btn_enabled; set => SetProperty(ref btn_enabled, value); }
        public bool IsAdm { get => isAdm; set => SetProperty(ref isAdm, value); }

        public string ButtonText { get => btntext; set => SetProperty(ref btntext, value); }
        /// <summary>
        /// Изменить = да, создать = нет
        /// </summary>
        public bool Mode { get => mode; set => SetProperty(ref mode, value);  }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //if (!Mode)
            //    Models.SharedModel.InvokeNewWorkerCreate(new Classes.NpcWorker() { Name = FIO, IsAdmin = isAdm });
            //else
            this.DialogResult = true;
            Close();
        }
        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, newValue))
            {
                field = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }
            return false;
        }

        private void Pass_tb_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrWhiteSpace(Pass_tb.Password))
            {
                Password = Pass_tb.Password;
            }
        }
    }
}
