using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
#pragma warning disable CS0067
namespace Project_smuzi.Classes
{
    public class Product : Element, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        [JsonIgnore]
        private ObservableCollection<Product> products;
        [JsonIgnore]
        private ObservableCollection<Element> elements;
        [JsonIgnore]
        private ObservableCollection<int> contaiment;

        [JsonIgnore]
        public ObservableCollection<Product> Products { get => products; set { products = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Productes")); } }
        [JsonIgnore]
        public ObservableCollection<Element> Elements { get => elements; set { elements = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Elementes")); } }
        public ObservableCollection<int> Contaiment
        {
            get { return contaiment; } //return new ObservableCollection<int>(Products.Select(t => t.BaseId).Concat(elements.Select(t => t.BaseId)));
            set { contaiment = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Contaiment")); }
        }

        public string PathTo { get; set; }
        [JsonIgnore]
        public string FolderTo => Path.GetDirectoryName(PathTo);
        public int DeepLevel { get; set; }

        public Product(string ident) : base(ident)
        {
            InitializeComponent();
        }
        public Product() : base()
        {
            InitializeComponent();
        }
        private void InitializeComponent()
        {
            Elements = new ObservableCollection<Element>();
            Products = new ObservableCollection<Product>();
            Contaiment = new ObservableCollection<int>();
            DeepLevel = 0;
        }
        [JsonIgnore]
        public IList<object> Items
        {
            get
            {
                IList<object> childNodes = new ObservableCollection<object>();
                foreach (var group in this.Products)
                    childNodes.Add(group);
                foreach (var entry in this.Elements)
                    childNodes.Add(entry);

                return childNodes;
            }
        }
    }
}
