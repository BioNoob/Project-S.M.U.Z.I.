using System.ComponentModel;
using System.Windows;

namespace Project_smuzi.Controls
{
    /// <summary>
    /// Логика взаимодействия для NewUserControl.xaml
    /// </summary>
    public partial class NewUserControl : Window, INotifyPropertyChanged
    {
        public NewUserControl()
        {
            InitializeComponent();
        }

        public string FIO { get; set; }
        public bool isAdm { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Models.SharedModel.InvokeNewWorkerCreate(new Classes.NpcWorker() { Name = FIO, IsAdmin = isAdm });
            Close();
        }
    }
}
