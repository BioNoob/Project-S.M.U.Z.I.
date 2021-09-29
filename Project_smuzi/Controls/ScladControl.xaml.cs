using Project_smuzi.Models;
using System.Windows;

namespace Project_smuzi.Controls
{
    /// <summary>
    /// Логика взаимодействия для ScladControl.xaml
    /// </summary>
    public partial class ScladControl : Window
    {
        public ScladControl()
        {
            InitializeComponent();
            SharedModel.CloseEvent += SharedModel_CloseEvent; ;
            this.Closing += ScladControl_Closing;
        }

        private void ScladControl_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        private void SharedModel_CloseEvent()
        {
            this.Closing -= ScladControl_Closing;
            this.Close();
        }
    }
}
