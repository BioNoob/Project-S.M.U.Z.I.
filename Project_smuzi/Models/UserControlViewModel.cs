using Project_smuzi.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace Project_smuzi.Models
{
    public class UserControlViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public DataBase DB => SharedModel.DB;
        public UserControlViewModel()
        {
            Groups = new ObservableCollection<NpcSector>();
            var tg = new NpcSector() { SectorLabel = "тест" };
            tg.SectorWorkers.Add(new NpcWorker() { Name = "STAS", IsAdmin = true });
            tg.SectorWorkers.Add(new NpcWorker() { Name = "Дударенко Светлана николаевна", IsAdmin = false });
            tg.SectorWorkers.Add(new NpcWorker() { Name = "Соколенко Артем Сергеевич", IsAdmin = false });
            Groups.Add(tg);
            tg = new NpcSector() { SectorLabel = "тест2" };
            tg.SectorWorkers.Add(new NpcWorker() { Name = "STAS3", IsAdmin = true });
            tg.SectorWorkers.Add(new NpcWorker() { Name = "STAS4", IsAdmin = false });
            tg.SectorWorkers.Add(new NpcWorker() { Name = "STAS5", IsAdmin = false });
            Groups.Add(tg);
        }

        private ObservableCollection<NpcSector> _groups;
        public ObservableCollection<NpcSector> Groups
        {
            get
            {
                return _groups;
            }
            set
            {
                _groups = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Groups"));
            }
        }

    }
}
