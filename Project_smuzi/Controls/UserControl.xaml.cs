using Project_smuzi.Models;
using System.Windows;

namespace Project_smuzi.Controls
{
    /// <summary>
    /// Логика взаимодействия для UserControl.xaml
    /// </summary>
    public partial class UserControl : Window
    {

        public UserControl()
        {
            InitializeComponent();
            SharedModel.CloseEvent += SharedModel_CloseEvent;
            this.Closing += UserControl_Closing;
        }

        private void UserControl_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        private void SharedModel_CloseEvent()
        {
            this.Closing -= UserControl_Closing;
            this.Close();
        }
    }
}
