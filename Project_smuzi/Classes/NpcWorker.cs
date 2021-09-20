using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
#pragma warning disable CS0067
namespace Project_smuzi.Classes
{
    public class NpcWorker : INotifyPropertyChanged
    {
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
        public List<string> Sectors { get; set; }
        public NpcWorker()
        {
            Sectors = new List<string>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
