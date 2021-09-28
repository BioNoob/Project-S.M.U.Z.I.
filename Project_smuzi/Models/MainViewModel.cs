using Project_smuzi.Classes;
using Project_smuzi.Controls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace Project_smuzi.Models
{
    public class MainViewModel : INotifyPropertyChanged
    {
        ProductInfo pi = new ProductInfo();
        private ObservableCollection<Product> selector;
        public ObservableCollection<Product> Selector
        {
            get => selector;
            set
            {
                selector = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Selector"));
            }
        }
        public MainViewModel()
        {
            Selector = new ObservableCollection<Product>();
            if (SharedModel.CurrentUser.IsAdmin)
                IsAdmin = Visibility.Visible;
            else
                IsAdmin = Visibility.Collapsed;
            SharedModel.ReadDataDone += SharedModel_ReadDataDone;
            SharedModel.LoadDataBase();
            SharedModel.OpenInfoEvent += SharedModel_OpenInfoEvent;

        }

        private void SharedModel_ReadDataDone()
        {
            //Selector = SharedModel.DB.Productes;
            DB.WorkProcStep += DB_WorkProcStep;
            //SharedModel.DB.WorkProcStep += DB_WorkProcStep;
            if (!SharedModel.CurrentUser.IsAdmin)
            {
                DB.Productes.Clear();
                foreach (var item in SharedModel.CurrentUser.WorkerGroups)
                {
                    foreach (var prod in item.SectorProducts)
                    {
                        DB.Productes.Add(SharedModel.DB.Productes.FirstOrDefault(t => t.BaseId == prod));
                    }
                }
            }

            Selector = DB.Productes;
            DeepLvl = 0;
            SearchText = "";
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Selector"));
        }

        private void DB_WorkProcStep()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            { /* Your code here */
                SharedModel.DB = DB;
                Selector = DB.Productes;
            }));

        }

        private void SharedModel_OpenInfoEvent(Product product)
        {
            pi.Show();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public DataBase DB { get => SharedModel.DB; }
        private int deepLvl;
        public int DeepLvl
        {
            get => deepLvl;
            set
            {
                deepLvl = value;
                SearchText = SearchText;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DeepLvl"));
            }
        }
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                var o = DeepLvl;
                if (DB != null)
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        Selector = new ObservableCollection<Product>(DB.Productes.Where(t => t.DeepLevel <= o));
                    }
                    else
                    {
                        ObservableCollection<Product> a = new ObservableCollection<Product>(DB.Productes.Where(t => t.ToXString.Contains(value) & t.DeepLevel <= o));
                        Selector = a;
                    }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Selector"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SearchText"));
            }
        }

        private string _prefix;
        public string Prefix
        {
            get => _prefix;
            set
            {
                _prefix = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Prefix"));
                SharedModel.DB.Prefix = value;
            }
        }

        public System.Windows.Visibility IsAdmin
        {
            get => isAdmin;
            set
            {
                isAdmin = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsAdmin"));
            }
        }


        private CommandHandler _startread;
        private Visibility isAdmin;

        public CommandHandler StartReadCommand
        {
            get
            {
                return _startread ?? (_startread = new CommandHandler(async obj =>
                {
                    FolderBrowserDialog ofd = new FolderBrowserDialog();
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        SharedModel.InvokeReadDataStart();
                        //SharedModel.DB.Clear();
                        DB.Clear();
                        Selector.Clear();
                        await Task.Run(() => DataBase.TestSpwReader(ofd.SelectedPath,DB));
                    }
                }//,
                 //(obj) => string.IsNullOrEmpty(obj.ToString())
                ));
            }
        }
    }
}
