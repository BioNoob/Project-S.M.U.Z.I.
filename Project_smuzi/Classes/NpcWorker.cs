using System.Collections.Generic;
using System.ComponentModel;
#pragma warning disable CS0067
namespace Project_smuzi.Classes
{
    public class NpcWorker : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public List<NpcSector> Sectors { get; set; }
        public NpcWorker()
        {
            Sectors = new List<NpcSector>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
