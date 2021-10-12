using Newtonsoft.Json;
using Project_smuzi.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
#pragma warning disable CS0067
namespace Project_smuzi.Classes
{
    public class Element : INotifyPropertyChanged
    {

        public static int Identificator = 1;
        [JsonIgnore]
        public string GetUriImg
        {
            get
            {
                string TheImageYouWantToShow = Section_id switch
                {
                    5 => "/Resources/документ_16.png",
                    10 => "/Resources/icons8-электроника-16.png",
                    15 => "/Resources/сборочные_16.png",
                    20 => "/Resources/деталь_16.png",
                    25 => "/Resources/стандарт_16.png",
                    30 => "/Resources/прочие_16.png",
                    35 => "/Resources/материал_16.png",
                    40 => "/Resources/комплект_16.png",
                    _ => "/Resources/прочие_16.png",
                };
                return TheImageYouWantToShow;
            }
        }
        public string Identification { get; set; }
        public string Name { get; set; }
        public int Section_id { get; set; }
        public int BaseId { get; set; }
        public double Count { get; set; }
        [JsonIgnore]
        private ObservableCollection<int> contaiments_in;

        public bool IsAdditional { get; set; }

        public ObservableCollection<int> Contaiments_in { get => contaiments_in; set { contaiments_in = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Contaiments_in")); } }

        [JsonIgnore]
        public Dictionary<Product, double> Contaiments_in_prod
        {
            get
            {
                Dictionary<Product, double> prd = new Dictionary<Product, double>();
                foreach (var item in Contaiments_in)
                {
                    var r = SharedModel.DB.Productes.FirstOrDefault(t => t.BaseId == item);
                    if (r != null)
                        if (!prd.ContainsKey(r))
                        {
                            var rs = r.Contaiment[this.BaseId];
                            prd.Add(r,rs);
                        }   
                }
                return prd;
            }
        }

        public string ToXString
        {
            get
            {
                string a = Name.Replace("\n", " ");
                string b = Identification.Replace("\n", " ");
                a = a.Trim();
                b = b.Trim();
                return $"{a} {b}";
            }
        }

        public Element(string ident)
        {
            Identification = ident;
            InitializeComponent();
        }
        private void InitializeComponent()
        {
            Section_id = 30;
            BaseId = Identificator++;
            Count = 0;
            Contaiments_in = new ObservableCollection<int>();
        }
        public Element()
        {
            Identification = "";
            InitializeComponent();
        }
        public string Section => SharedModel.Sections[Section_id];

        public event PropertyChangedEventHandler PropertyChanged;

        public Element Copy()
        {
            return new Element()
            {
                Section_id = this.Section_id,
                BaseId = this.BaseId,
                Contaiments_in = this.Contaiments_in,
                Count = this.Count,
                Identification = this.Identification,
                Name = this.Name,
                IsAdditional = this.IsAdditional
            };
        }
    }
}
