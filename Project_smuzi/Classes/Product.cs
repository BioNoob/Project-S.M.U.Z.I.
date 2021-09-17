using Newtonsoft.Json;
using Project_smuzi.Controls;
using Project_smuzi.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
        private Dictionary<int, double> contaiment;

        [JsonIgnore]
        public ObservableCollection<Product> Products { get => products; set { products = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Productes")); } }
        [JsonIgnore]
        public ObservableCollection<Element> Elements { get => elements; set { elements = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Elementes")); } }
        /// <summary>
        /// ID element based, count
        /// </summary>
        public Dictionary<int,double> Contaiment
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
            Contaiment = new Dictionary<int, double>();
            DeepLevel = 0;
        }

        public new Product Copy()
        {
            var cp = new Product()
            {
                BaseId = this.BaseId,
                Contaiment = this.Contaiment,
                Count = this.Count,
                Section_id = this.Section_id,
                Products = this.Products,
                Name = this.Name,
                PathTo = this.PathTo,
                Identification = this.Identification,
                Elements = this.Elements,
                DeepLevel = this.DeepLevel,
                Contaiments_in = this.Contaiments_in
            };
            return cp;
        }

        [JsonIgnore]
        public IList<object> Items
        {
            get
            {
                IList<object> childNodes = new ObservableCollection<object>();
                foreach (var group in this.Products.OrderByDescending(t=>t.Products.Count))
                    childNodes.Add(group);
                foreach (var entry in this.Elements)
                    childNodes.Add(entry);

               

                return childNodes;
            }
        }
        [JsonIgnore]
        private CommandHandler _openInfo;
        [JsonIgnore]
        public CommandHandler OpenInfoCommand
        {
            get
            {
                return _openInfo ?? (_openInfo = new CommandHandler(obj =>
                {
                    SharedModel.InvokeOpenInfoEvent(this);
                },
                (obj) => true
                ));
            }
        }
    }
}
