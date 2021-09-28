using Project_smuzi.Classes;
using Project_smuzi.Models;
using System;
using System.ComponentModel;
using System.Windows;
#pragma warning disable CS0067

namespace Project_smuzi
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string FolderPath = string.Empty;
        public MainWindow()
        {
            InitializeComponent();
            TB.Text = "ТСЮИ";
            SharedModel.ReadDataDone += MVM_ReadDataDone;
            SharedModel.ReadDataStart += MVM_ReadDataStart;
            Closing += MainWindow_Closing;

        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            SharedModel.InvokeCloseApp();
            e.Cancel = false;
        }

        private void MVM_ReadDataStart()
        {
            this.Search_tb.IsEnabled = false;
            this.Deeb_cmb.IsEnabled = false;
            this.TB.IsEnabled = false;
            this.Start_btn.IsEnabled = false;
            Img_refresh.Visibility = Visibility.Visible;
        }

        private void MVM_ReadDataDone(DataBase db)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {

                this.Start_btn.IsEnabled = true;
                this.Search_tb.IsEnabled = true;
                this.Deeb_cmb.IsEnabled = true;
                this.TB.IsEnabled = true;
                Deeb_cmb.SelectedIndex = Deeb_cmb.Items.Count - 1;
                Img_refresh.Visibility = Visibility.Collapsed;
            }));
        }
    }
}
