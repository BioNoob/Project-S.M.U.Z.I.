using Project_smuzi.Classes;
using Project_smuzi.Models;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

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
            SharedModel.OpenFolderEvent += SharedModel_OpenFolderEvent;
        }

        private void SharedModel_OpenFolderEvent(Product product)
        {
            if (!string.IsNullOrEmpty(product.FolderTo))
                Process.Start("explorer.exe", $"{product.FolderTo}");
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
    }
}
