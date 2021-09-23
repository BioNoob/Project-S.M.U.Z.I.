using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
#pragma warning disable CS0067
namespace Project_smuzi.Classes
{
    public class NpcWorker : INotifyPropertyChanged
    {
        public static int Identificator = 1;



        private ObservableCollection<int> sectors;

        [JsonIgnore]
        public string GetImg
        {
            get
            {
                if (IsAdmin)
                    return "/Resources/админ_16.png";
                else
                    return "/Resources/юзер_16.png";
            }
        }
        private bool isAdmin;
        public bool IsAdmin { get => isAdmin; set => SetProperty(ref isAdmin, value); }
        private string name;
        public string Name { get => name; set => SetProperty(ref name, value); }
        private int workerId;
        public int WorkerId { get => workerId; set => SetProperty(ref workerId, value); }

        public ObservableCollection<int> Sectors { get => sectors; set => SetProperty(ref sectors, value); }
        [JsonIgnore]
        private NpcBase owner;
        [JsonIgnore]
        public NpcBase Owner { get => owner; set => SetProperty(ref owner, value); }

        [JsonIgnore]
        public ObservableCollection<NpcSector> WorkerGroups
        {
            get
            {
                return new ObservableCollection<NpcSector>(Owner.Groups.Where(t => t.SectorWorkers.Contains(WorkerId)));
            }
        }


        public NpcWorker()
        {
            Sectors = new ObservableCollection<int>();
            WorkerId = Identificator++;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [JsonIgnore]
        private CommandHandler _deletefromgroup;
        [JsonIgnore]
        public CommandHandler DeleteFromGroupCommand
        {
            get
            {
                return _deletefromgroup ??= new CommandHandler(obj =>
                {
                    Models.SharedModel.InvokeWorkerDeleteFromGroup(this);
                },
                (obj) => true
                );
            }
        }
        [JsonIgnore]
        private CommandHandler _deletefrom;
        [JsonIgnore]
        public CommandHandler DeleteFromCommand
        {
            get
            {
                return _deletefrom ??= new CommandHandler(obj =>
                {
                    Models.SharedModel.InvokeWorkerDelete(this);
                },
                (obj) => true
                );
            }
        }
        [JsonIgnore]
        private CommandHandler _edituser;


        [JsonIgnore]
        public CommandHandler EditUserCommand
        {
            get
            {
                return _edituser ??= new CommandHandler(obj =>
                {
                    Models.SharedModel.InvokeWorkerEdit(this);
                },
                (obj) => true
                );
            }
        }

        public void SetWorkerSectors(NpcSector sector)
        {
            this.Sectors.Add(sector.SectorId);
            sector.SectorWorkers.Add(this.WorkerId);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("WorkerGroups"));
            
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
