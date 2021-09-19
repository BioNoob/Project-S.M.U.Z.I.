using System.Collections.ObjectModel;
using System.ComponentModel;
#pragma warning disable CS0067
namespace Project_smuzi.Classes
{
    public class NpcSector : INotifyPropertyChanged
    {
        public string SectorLabel { get; set; }
        public ObservableCollection<NpcWorker> SectorWorkers { get; set; }
        public ObservableCollection<int> SectorProducts { get; set; }
        public NpcSector()
        {
            SectorWorkers = new ObservableCollection<NpcWorker>();
            SectorProducts = new ObservableCollection<int>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
