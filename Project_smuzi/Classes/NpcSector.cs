using Newtonsoft.Json;
using Project_smuzi.Classes;
using Project_smuzi.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
#pragma warning disable CS0067
namespace Project_smuzi.Classes
{
    public class NpcSector : INotifyPropertyChanged
    {
        public static int Identificator = 1;
        public string SectorLabel { get => sectorLabel; set { SetProperty(ref sectorLabel, value); } }
        public ObservableCollection<int> SectorWorkers { get => sectorWorkers; set => SetProperty(ref sectorWorkers, value); }
        public ObservableCollection<int> SectorProducts { get => sectorProducts; set => SetProperty(ref sectorProducts, value); }

        [JsonIgnore]
        public ObservableCollection<NpcWorker> GroupWorkers
        {
            get
            {
                return new ObservableCollection<NpcWorker>(SharedModel.DB_Workers.Workers.Where(t => t.Sectors.Contains(SectorId)));
            }
        }
        public int SectorId { get => sectorId; set => SetProperty(ref sectorId, value); }
        public NpcSector()
        {
            SectorWorkers = new ObservableCollection<int>();
            SectorProducts = new ObservableCollection<int>();
            SectorId = Identificator++;
            //SharedModel.DB_Workers.GroupRequestUpdate += DB_Workers_GroupRequestUpdate;
            SharedModel.DB_Workers.PropertyChanged += DB_Workers_PropertyChanged;
        }

        private void DB_Workers_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Groups")
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("GroupWorkers"));
        }

        //private void DB_Workers_GroupRequestUpdate()
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("GroupWorkers"));
        //}

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

        public override string ToString()
        {
            return this.SectorLabel;
        }

        private CommandHandler deleteGroupCommand;
        public ICommand DeleteGroupCommand => deleteGroupCommand ??= new CommandHandler(DeleteGroup);

        private void DeleteGroup(object commandParameter)
        {
            if (System.Windows.Forms.MessageBox.Show($"Удалить сектор \"{SectorLabel}\"?", "Удаление сектора", System.Windows.Forms.MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
                SharedModel.DB_Workers.RemoveGroup(this);
        }

        private CommandHandler renameGropCommand;
        public ICommand RenameGropCommand => renameGropCommand ??= new CommandHandler(RenameGrop);

        private void RenameGrop(object commandParameter)
        {
            SharedModel.DB_Workers.RenameGroup(this);
        }
    }
}
