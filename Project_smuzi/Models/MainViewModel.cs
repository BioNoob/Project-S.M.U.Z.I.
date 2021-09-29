using Project_smuzi.Classes;
using Project_smuzi.Controls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
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

            SharedModel.OpenInfoEvent += SharedModel_OpenInfoEvent;

        }

        private void SharedModel_ReadDataDone(DataBase db)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                SharedModel.DB = db.Copy();
                DB_local = db.Copy();
                if (!SharedModel.CurrentUser.IsAdmin)
                {
                    DB_local.Productes.Clear();
                    foreach (var item in SharedModel.CurrentUser.WorkerGroups)
                    {
                        foreach (var prod in item.SectorProducts)
                        {
                            DB_local.Productes.Add(SharedModel.DB.Productes.FirstOrDefault(t => t.BaseId == prod));
                        }
                    }
                }
                Selector = new ObservableCollection<Product>(DB_local.Productes);
                DeepLvl = 0;
                SearchText = "";
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Selector"));
            }));
        }

        private void SharedModel_OpenInfoEvent(Product product)
        {
            pi.Show();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private DataBase _db;
        public DataBase DB_local { get => _db; set { _db = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DB_local")); } }
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
                if (DB_local != null)
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        Selector = new ObservableCollection<Product>(DB_local.Productes.Where(t => t.DeepLevel <= o));
                    }
                    else
                    {
                        Selector = new ObservableCollection<Product>(DB_local.Productes.Where(t => t.ToXString.Contains(value) & t.DeepLevel <= o));
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
                        //DB.Clear();
                        LogWindow lgw = new LogWindow();
                        lgw.Show();
                        Selector = new ObservableCollection<Product>();
                        if (string.IsNullOrWhiteSpace(Prefix))
                            System.Windows.Forms.MessageBox.Show("Префикс изделий не установлен!");

                        DataBase.worker.DoWork += worker_DoWork;
                        DataBase.worker.ProgressChanged += worker_ProgressChanged;
                        DataBase.worker.RunWorkerCompleted += worker_RunWorkerCompleted;
                        DataBase.worker.RunWorkerAsync(ofd.SelectedPath);
                    }
                }//,
                 //(obj) => string.IsNullOrEmpty(obj.ToString())
                ));
            }
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            //await Task.Run(() => DataBase.TestSpwReader((string)e.Argument, Prefix));
            DataBase.TestSpwReader((string)e.Argument, Prefix);
            e.Result = 0;
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                DB_local = new DataBase(SharedModel.DB);
                Selector = new ObservableCollection<Product>(DB_local.Productes); //ИМЕННО ЭТА ХЕРНЯ ВСЕ ЛОМАЕТ
            }
            ));
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //System.Windows.MessageBox.Show("Numbers between 0 and 10000 divisible by 7: " + e.Result);
        }
    }
}
