using Newtonsoft.Json;
using Project_smuzi.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;
#pragma warning disable CS0067
namespace Project_smuzi.Classes
{
    public class Element : INotifyPropertyChanged
    {
        private static Dictionary<int, string> Sections = new Dictionary<int, string>(8)
        {
            {5, "Документация"},
            {10, "Комплексы"},
            {15, "Сборочные единицы"},
            {20, "Детали"},
            {25, "Стандартные изделия"},
            {30, "Прочие изделия"},
            {35, "Материалы"},
            {40, "Комплекты"}
        };
        public static int Identificator = 1;
        public static string GetInterpritation(int id)
        {
            return Sections[id];
        }
        [JsonIgnore]
        public string GetUriImg
        {
            get
            {
                string TheImageYouWantToShow = "";
                switch (Section_id)
                {
                    case 5:
                        TheImageYouWantToShow = "/Resources/документ_16.png";
                        break;
                    case 10:
                        TheImageYouWantToShow = "/Resources/icons8-электроника-16.png";
                        break;
                    case 15:
                        TheImageYouWantToShow = "/Resources/сборочные_16.png";
                        break;
                    case 20:
                        TheImageYouWantToShow = "/Resources/деталь_16.png";
                        break;
                    case 25:
                        TheImageYouWantToShow = "/Resources/стандарт_16.png";
                        break;
                    case 30:
                        TheImageYouWantToShow = "/Resources/прочие_16.png";
                        break;
                    case 35:
                        TheImageYouWantToShow = "/Resources/материал_16.png";
                        break;
                    case 40:
                        TheImageYouWantToShow = "/Resources/комплект_16.png";
                        break;
                    default:
                        TheImageYouWantToShow = "/Resources/ico_refresh_48.png";
                        break;
                }
                return TheImageYouWantToShow;
            }
        }
        public string Identification { get; set; }
        public string Name { get; set; }
        public int Section_id { get; set; }
        public int BaseId { get; set; }
        public double Count { get; set; }

        private ObservableCollection<int> contaiments_in;


        public ObservableCollection<int> Contaiments_in { get => contaiments_in; set { contaiments_in = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Contaiments_in")); } }
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
        public string Section => Sections[Section_id];

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
                Name = this.Name
            };
        }
    }
}
