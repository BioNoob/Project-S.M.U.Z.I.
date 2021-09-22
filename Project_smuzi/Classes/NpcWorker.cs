using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
#pragma warning disable CS0067
namespace Project_smuzi.Classes
{
    public class NpcWorker : INotifyPropertyChanged
    {
        public static int Identificator = 1;

        private ObservableCollection<string> sectors;

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
        public bool IsAdmin { get => isAdmin; set => SetProperty(ref isAdmin, value); }
        public string Name { get => name; set => SetProperty(ref name, value); }
        public int WorkerId { get => workerId; set => SetProperty(ref workerId, value); }

        public ObservableCollection<string> Sectors { get => sectors; set => SetProperty(ref sectors, value); }
        public NpcWorker()
        {
            Sectors = new ObservableCollection<string>();
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
                return _deletefromgroup ?? (_deletefromgroup = new CommandHandler(obj =>
                {
                    Models.SharedModel.InvokeWorkerDeleteFromGroup(this);
                },
                (obj) => true
                ));
            }
        }
        [JsonIgnore]
        private CommandHandler _deletefrom;
        [JsonIgnore]
        public CommandHandler DeleteFromCommand
        {
            get
            {
                return _deletefrom ?? (_deletefrom = new CommandHandler(obj =>
                {
                    Models.SharedModel.InvokeWorkerDelete(this);
                },
                (obj) => true
                ));
            }
        }
        [JsonIgnore]
        private CommandHandler _edituser;
        private int id;
        private string name;
        private bool isAdmin;
        private int workerId;

        [JsonIgnore]
        public CommandHandler EditUserCommand
        {
            get
            {
                return _edituser ?? (_edituser = new CommandHandler(obj =>
                {
                    Models.SharedModel.InvokeWorkerEdit(this);
                },
                (obj) => true
                ));
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
