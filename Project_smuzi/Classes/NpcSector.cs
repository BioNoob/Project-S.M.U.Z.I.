using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
#pragma warning disable CS0067
namespace Project_smuzi.Classes
{
    public class NpcSector : INotifyPropertyChanged
    {
        public string SectorLabel { get; set; }
        public List<NpcWorker> SectorWorker { get; set; }
        public ObservableCollection<Product> SectorProducts { get; set; }
        public NpcSector()
        {
            SectorWorker = new List<NpcWorker>();
            SectorProducts = new ObservableCollection<Product>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
