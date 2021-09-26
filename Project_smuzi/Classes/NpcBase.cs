using Newtonsoft.Json;
using Project_smuzi.Properties;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Project_smuzi.Classes
{

    public class NpcBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public NpcBase()
        {
            Groups = new ObservableCollection<NpcSector>();
            Workers = new ObservableCollection<NpcWorker>();
            NpcSector.Identificator = 1;
            NpcWorker.Identificator = 1;
        }

        public delegate void UserEvents();//(NpcWorker user);
        //public delegate void GroupEvents();//(NpcSector sector);
        //public event GroupEvents GroupRequestUpdate;
        public event UserEvents UserWasDeleteEvent;

        public void AddWorker(NpcWorker nw)
        {
            Workers.Add(nw);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Workers"));
        }
        public void AddGroup(NpcSector nw)
        {
            Groups.Add(nw);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Groups"));
        }
        public void AddWorkerToGroup(NpcWorker nw, NpcSector nc)
        {
            Groups.FirstOrDefault(t => t.SectorId == nc.SectorId).SectorWorkers.Add(nw.WorkerId);
            Workers.FirstOrDefault(t => t.WorkerId == nw.WorkerId).Sectors.Add(nc.SectorId);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Workers"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Groups"));
        }
        public void AddWorkersToGroup(IEnumerable<NpcWorker> nw, NpcSector nc)
        {
            foreach (var item in nw)
            {
                Groups.FirstOrDefault(t => t.SectorId == nc.SectorId).SectorWorkers.Add(item.WorkerId);
                Workers.FirstOrDefault(t => t.WorkerId == item.WorkerId).Sectors.Add(nc.SectorId);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Workers"));
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Groups"));
        }
        public void RemoveWorkerFromGroup(NpcWorker nw, NpcSector nc)
        {
            Groups.FirstOrDefault(t => t.SectorId == nc.SectorId).SectorWorkers.Remove(nw.WorkerId);
            Workers.FirstOrDefault(t => t.WorkerId == nw.WorkerId).Sectors.Remove(nc.SectorId);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Workers"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Groups"));
        }
        public void RemoveGroup(NpcSector nc)
        {
            Groups.Remove(nc);
            Workers.Where(t => t.Sectors.Contains(nc.SectorId)).ToList().ForEach(t => t.Sectors.Remove(nc.SectorId));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Workers"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Groups"));
        }
        public void RenameGroup(NpcSector nc)
        {
            var z = Groups.FirstOrDefault(t => t.SectorId == nc.SectorId);
            if (z != null)
            {
                z.SectorLabel = nc.SectorLabel;
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Workers"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Groups"));
        }
        public void ChangeWorker(NpcWorker nw)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Workers"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Groups"));
        }
        public void RemoveWorker(NpcWorker nw)
        {
            Workers.Remove(nw);
            Groups.Where(t => t.SectorWorkers.Contains(nw.WorkerId)).ToList().ForEach(t => t.SectorWorkers.Remove(nw.WorkerId));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Workers"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Groups"));
            UserWasDeleteEvent?.Invoke();
        }

        private ObservableCollection<NpcSector> _groups;
        public ObservableCollection<NpcSector> Groups { get => _groups; set => SetProperty(ref _groups, value); } 

        private ObservableCollection<NpcWorker> _workers;

        public ObservableCollection<NpcWorker> Workers { get => _workers; set => SetProperty(ref _workers, value); }

        public void SaveJson()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            settings.ContractResolver = new EncryptedStringPropertyResolver("My-Sup3r-Secr3t-Key");

            string t = JsonConvert.SerializeObject(this, Formatting.Indented, settings);
            Settings.Default.NPC_DB_json = t;
            Settings.Default.Save();
        }
        public static NpcBase LoadFromJson()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            settings.ContractResolver = new EncryptedStringPropertyResolver("My-Sup3r-Secr3t-Key");



            if (!string.IsNullOrEmpty(Settings.Default.NPC_DB_json))
                return JsonConvert.DeserializeObject<NpcBase>(Settings.Default.NPC_DB_json, settings);
            else
            {
                var b = new NpcBase();
                var a = new NpcWorker();
                a.Name = "Admin";
                a.Password = "2012";
                a.IsAdmin = true;
                b.AddWorker(a);
                return b;
            }

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
    }
}
