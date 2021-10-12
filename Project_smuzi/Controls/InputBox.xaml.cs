using Project_smuzi.Models;
using System.Windows;

namespace Project_smuzi.Controls
{
    /// <summary>
    /// Логика взаимодействия для InputBox.xaml
    /// </summary>
    public partial class InputBox : Window
    {
        public InputBox(string InputCuption)
        {
            InitializeComponent();
            model = (InputBoxViewModel)DataContext;
            model.Request = InputCuption;
        }
        InputBoxViewModel model;
        public string Answer => model.Input;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
        public string ShowDialog_()
        {
            if (base.ShowDialog() == true)
                return Answer;
            else
                return "";
        }

    }
}
