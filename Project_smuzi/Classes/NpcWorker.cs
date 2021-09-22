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
        public bool IsAdmin { get; set; }
        public string Name { get; set; }
        public ObservableCollection<string> Sectors { get => sectors; set => SetProperty(ref sectors, value); }
        public NpcWorker()
        {
            Sectors = new ObservableCollection<string>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

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
