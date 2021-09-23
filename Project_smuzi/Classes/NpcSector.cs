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
        [JsonIgnore]
        private NpcBase owner;
        [JsonIgnore]
        public NpcBase Owner { get => owner; set => SetProperty(ref owner, value); }

        public static int Identificator = 1;
        public string SectorLabel { get => sectorLabel; set => SetProperty(ref sectorLabel, value); }
        public ObservableCollection<int> SectorWorkers { get => sectorWorkers; set => SetProperty(ref sectorWorkers, value); }
        public ObservableCollection<int> SectorProducts { get => sectorProducts; set => SetProperty(ref sectorProducts, value); }

        [JsonIgnore]
        public ObservableCollection<NpcWorker> GroupWorkers
        {
            get
            {
                return new ObservableCollection<NpcWorker>(Owner.Workers.Where(t => t.Sectors.Contains(SectorId)));
            }
        }
        public int SectorId { get => sectorId; set => SetProperty(ref sectorId, value); }
        public NpcSector(NpcBase own)
        {
            Owner = own;
            SectorWorkers = new ObservableCollection<int>();
            SectorProducts = new ObservableCollection<int>();
            SectorId = Identificator++;
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
        private int sectorId;

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
                return _deletegroup ??= new CommandHandler(obj =>
                {
                    Models.SharedModel.InvokeGroupDelete(this);
                },
                (obj) => true
                );
            }
        }
        public void SetWorkerGroupByThis(IEnumerable<NpcWorker> workers)
        {
            foreach (var item in workers)
            {
                item.Sectors.Add(this.SectorId);
                SectorWorkers.Add(item.WorkerId);
            }
        }
        public void SetWorkerGroupByThis(NpcWorker worker)
        {
                SectorWorkers.Add(worker.WorkerId);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("GroupWorkers"));
        }
        public override string ToString()
        {
            return this.SectorLabel;
        }

    }
}
