using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
#pragma warning disable CS0067
namespace Project_smuzi.Classes
{
    public class NpcSector : INotifyPropertyChanged
    {
        public string SectorLabel { get => sectorLabel; set => SetProperty(ref sectorLabel, value); }
        public ObservableCollection<int> SectorWorkers { get => sectorWorkers; set => SetProperty(ref sectorWorkers, value); }
        public ObservableCollection<int> SectorProducts { get => sectorProducts; set => SetProperty(ref sectorProducts, value); }
        public NpcSector()
        {
            SectorWorkers = new ObservableCollection<int>();
            SectorProducts = new ObservableCollection<int>();
        }

        public ObservableCollection<Product> GetProductsOf(DataBase DB)
        {
            var q = new List<Product>();
            foreach (var item in SectorProducts)
            {
                q.Add(DB.Productes.Where(t => t.BaseId == item).FirstOrDefault());
            }
            return new ObservableCollection<Product>(q);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private ObservableCollection<int> sectorWorkers;
        private ObservableCollection<int> sectorProducts;
        private string sectorLabel;

        public delegate void ItemClick();
        public event ItemClick ItemClickEvent;
        public void ItemClickEventInvoke()
        {
            ItemClickEvent?.Invoke();
        }

        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, newValue))
            {
                field = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }
            return false;
        }

        [JsonIgnore]
        private CommandHandler _deletegroup;
        [JsonIgnore]
        public CommandHandler DeleteGroupCommand
        {
            get
            {
                return _deletegroup ?? (_deletegroup = new CommandHandler(obj =>
                {
                    Models.SharedModel.InvokeGroupDelete(this);
                },
                (obj) => true
                ));
            }
        }


    }
}
