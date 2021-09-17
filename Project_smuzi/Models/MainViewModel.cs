using Kompas6Constants;
using KompasAPI7;
using Newtonsoft.Json;
using Project_smuzi.Classes;
using Project_smuzi.Controls;
using Project_smuzi.Properties;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Project_smuzi.Models
{
    public class MainViewModel : INotifyPropertyChanged
    {
        ProductInfo pi = new ProductInfo();
        public MainViewModel()
        {
            SharedModel.LoadDataBase();
            SharedModel.OpenInfoEvent += SharedModel_OpenInfoEvent;
        }

        private void SharedModel_OpenInfoEvent(Product product)
        {
            pi.Show();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public DataBase DB
        {
            get { return SharedModel.DB; }
            //set { _db = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DB")); }
        }
        private string _prefix;
        public string Prefix
        {
            get => _prefix; 
            set
            {
                _prefix = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Prefix"));
                SharedModel.DB.Prefix = _prefix;
            }
        }

        //Вынести в статическую модель? или в датабейз вообще


        private CommandHandler _startread;
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
                        await Task.Run(() => DB.TestSpwReader(ofd.SelectedPath));
                    }
                }//,
                 //(obj) => string.IsNullOrEmpty(obj.ToString())
                ));
            }
        }
    }
}
