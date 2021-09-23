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
        private bool isAdm1;
        private string btntext;
        private string fIO;

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

        private bool btn_enabled;
        public bool Btn_enabled { get => btn_enabled; set => SetProperty(ref btn_enabled, value); }
        public bool isAdm { get => isAdm1; set => SetProperty(ref isAdm1, value); }

        public string ButtonText { get => btntext; set => SetProperty(ref btntext, value); }
        /// <summary>
        /// Изменить = да, создать = нет
        /// </summary>
        public bool Mode { get => mode; set => SetProperty(ref mode, value);  }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!Mode)
                Models.SharedModel.InvokeNewWorkerCreate(new Classes.NpcWorker() { Name = FIO, IsAdmin = isAdm });
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
    }
}
