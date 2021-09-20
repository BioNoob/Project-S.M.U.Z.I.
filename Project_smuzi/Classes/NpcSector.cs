using Newtonsoft.Json;
using Project_smuzi.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
#pragma warning disable CS0067
namespace Project_smuzi.Classes
{
    public class NpcSector : INotifyPropertyChanged
    {
        public string SectorLabel { get; set; }
        public List<NpcWorker> SectorWorkers { get; set; }
        public List<int> SectorProducts { get; set; }
        public NpcSector()
        {
            SectorWorkers = new List<NpcWorker>();
            SectorProducts = new List<int>();
        }
        [JsonIgnore]
        private CommandHandler _addusertocommand;
        [JsonIgnore]
        public CommandHandler AddUserToCommand
        {
            get
            {
                return _addusertocommand ?? (_addusertocommand = new CommandHandler(obj =>
                {
                    var t = obj;
                },
                (obj) => true
                ));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
