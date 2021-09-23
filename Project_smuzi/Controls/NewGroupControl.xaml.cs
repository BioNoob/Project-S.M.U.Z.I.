using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Project_smuzi.Controls
{
    /// <summary>
    /// Логика взаимодействия для NewGroupControl.xaml
    /// </summary>
    public partial class NewGroupControl : Window, INotifyPropertyChanged
    {
        public NewGroupControl()
        {
            InitializeComponent();
            GroupName = "";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private string groupName;
        public string GroupName { get => groupName; set { SetProperty(ref groupName, value); if (string.IsNullOrWhiteSpace(value)) Btn_enabled = false; else Btn_enabled = true; } }

        private bool btn_enabled;
        public bool Btn_enabled { get => btn_enabled; set => SetProperty(ref btn_enabled, value); }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
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
