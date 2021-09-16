using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
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
        private static int Identificator = 1;
        public static string GetInterpritation(int id)
        {
            return Sections[id];
        }
        public string Identification { get; set; }
        public string Name { get; set; }
        public int Section_id { get; set; }
        public int BaseId { get; set; }
        public int? Count { get; set; }

        private ObservableCollection<int> contaiments_in;
        public ObservableCollection<int> Contaiments_in { get => contaiments_in; set { contaiments_in = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Contaiments_in")); } }
        public string ToXString
        {
            get
            {
                string a = Name.Replace("\n", " ");
                string b = Identification.Replace("\n", " ");
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
            Count = null;
            Contaiments_in = new ObservableCollection<int>();
        }
        public Element()
        {
            Identification = "";
            InitializeComponent();
        }
        public string Section => Sections[Section_id];

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
