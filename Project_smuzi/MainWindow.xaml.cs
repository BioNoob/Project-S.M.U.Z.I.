using Kompas6Constants;
using KompasAPI7;
using Newtonsoft.Json;
using Project_smuzi.Classes;
using Project_smuzi.Models;
using Project_smuzi.Properties;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
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
        private MainViewModel MVM => (MainViewModel)DataContext;
        public MainWindow()
        {
            InitializeComponent();
            TB.Text = "ТСЮИ";

            //DB = new DataBase();
            //Productes = new List<Product>();
            //Elementes = new List<Element>();

            ComboBox_SelectionChanged(null, null);
            MVM.ReadDataDone += MVM_ReadDataDone;
            MVM.ReadDataStart += MVM_ReadDataStart;

        }
        public (string s, int i) SwitchRepresentParameter
        {
            get => switchRepresentParameter;
            set
            {
                switchRepresentParameter = value;
            }
        }
        private (string s, int i) switchRepresentParameter;

        private void MVM_ReadDataStart()
        {
            this.Search_tb.IsEnabled = false;
            this.Deeb_cmb.IsEnabled = false;
            this.TB.IsEnabled = false;
            this.Start_btn.IsEnabled = false;
            Img_refresh.Visibility = Visibility.Visible;
        }

        private void MVM_ReadDataDone()
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

        private void Search_tb_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (Deeb_cmb.SelectedIndex < 0)
            {
                Deeb_cmb.SelectionChanged -= ComboBox_SelectionChanged;
                Deeb_cmb.SelectedIndex = 0;
                Deeb_cmb.SelectionChanged += ComboBox_SelectionChanged;
            }
            MVM.SelectorSwitch(Search_tb.Text, (int)Deeb_cmb.SelectedItem); // По идее тут должна быть комманда в привязке
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Search_tb_TextChanged(null, null);
        }

        private void TextBlock_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ContextMenu cm = FindResource("cmProduct") as ContextMenu;
            cm.PlacementTarget = sender as UIElement;
            cm.IsOpen = true;
            cm.Tag = ((TextBlock)sender).Text;

        }

        private void MenuItem_Folder_Click(object sender, RoutedEventArgs e)
        {
            var a = (Product)((MenuItem)sender).DataContext;
            Process.Start("explorer.exe", $"{a.FolderTo}");
        }
    }
}
