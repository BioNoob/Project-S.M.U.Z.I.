using Project_smuzi.Classes;
using Project_smuzi.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Логика взаимодействия для ProductInfo.xaml
    /// </summary>
    public partial class ProductInfo : Window
    {
        public ProductInfo()
        {
            InitializeComponent();
            this.Closing += ProductInfo_Closing;
            SharedModel.CloseEvent += SharedModel_CloseEvent;
        }

        private void SharedModel_CloseEvent()
        {
            this.Closing -= ProductInfo_Closing;
            this.Close();
        }

        private void ProductInfo_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true; 
        }



        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var a = (Product)((MenuItem)sender).DataContext;
            if (!string.IsNullOrEmpty(a.FolderTo))
                Process.Start("explorer.exe", $"{a.FolderTo}");
        }
    }
}
