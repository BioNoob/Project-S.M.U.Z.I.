using Newtonsoft.Json;
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
        public new event PropertyChangedEventHandler PropertyChanged;
        //[JsonIgnore]
        //private ObservableCollection<Product> products;
        //[JsonIgnore]
        //private ObservableCollection<Element> elements;
        [JsonIgnore]
        private Dictionary<int, double> contaiment;

        //[JsonIgnore]
        //public ObservableCollection<Product> Products
        //{ 
        //    get
        //    {
        //        ObservableCollection<Product> p = new ObservableCollection<Product>();
        //        foreach (var item in contaiment.Keys)
        //        {
        //            var a = SharedModel.DB.Productes.FirstOrDefault(t => t.BaseId == item);
        //            if (a != null)
        //            {
        //                a.Count = contaiment[item];
        //                p.Add(a);
        //            }
        //        }
        //        return p;
        //    }
        //}
        //[JsonIgnore]
        //public ObservableCollection<Element> Elements
        //{
        //    get
        //    {
        //        ObservableCollection<Element> p = new ObservableCollection<Element>();
        //        foreach (var item in contaiment.Keys)
        //        {
        //            var a = SharedModel.DB.Elementes.FirstOrDefault(t => t.BaseId == item);
        //            if(a != null)
        //            {
        //                a.Count = contaiment[item];
        //                p.Add(a);
        //            }
        //        }
        //        return p;
        //    }
        //}

        public void GoingDeeper()
        {
            foreach (var item in contaiment.Keys)
            {
                var a = SharedModel.DB.Productes.FirstOrDefault(t => t.BaseId == item);
                if (a != null)
                    a.DeepLevel++;
            }
        }

        /// <summary>
        /// ID element based, count
        /// </summary>
        public Dictionary<int, double> Contaiment
        {
            get { return contaiment; } //return new ObservableCollection<int>(Products.Select(t => t.BaseId).Concat(elements.Select(t => t.BaseId)));
            set { contaiment = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Contaiment")); }
        }

        [JsonIgnore]
        public ObservableCollection<Element> Contaiments_elements_in_prod
        {
            get
            {
                ObservableCollection<Element> prd = new ObservableCollection<Element>();
                foreach (var item in Contaiment.Keys)
                {
                    var r = SharedModel.DB.Elementes.Where(t => t.BaseId == item).FirstOrDefault();
                    if (r != null)
                        prd.Add(r);
                }
                return prd;
            }
        }
        [JsonIgnore]
        public ObservableCollection<Product> Contaiments_products_in_prod
        {
            get
            {
                ObservableCollection<Product> prd = new ObservableCollection<Product>();
                foreach (var item in Contaiment.Keys)
                {
                    var r = SharedModel.DB.Productes.Where(t => t.BaseId == item).FirstOrDefault();
                    if (r != null)
                        prd.Add(r);
                }
                return prd;
            }
        }

        public string PathTo { get; set; }
        [JsonIgnore]
        public string FolderTo => Path.GetDirectoryName(PathTo);
        [JsonIgnore]
        public bool SomeItemSelected { get; set; }
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
                //Products = this.Products,
                Name = this.Name,
                PathTo = this.PathTo,
                Identification = this.Identification,
                //Elements = this.Elements,
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
                foreach (var prd in Contaiment.Keys)
                {
                    var t = SharedModel.DB.Productes.FirstOrDefault(t => t.BaseId == prd);
                    if (t != null)
                    {
                        Product b = t.Copy();
                        b.Count = Contaiment[prd];
                        childNodes.Add(b);
                    }

                    else
                    {
                        var s = SharedModel.DB.Elementes.FirstOrDefault(t => t.BaseId == prd);
                        if (s != null)
                        {
                            Element a = s.Copy();
                            a.Count = Contaiment[prd];
                            childNodes.Add(a);
                        }
                            
                    }
                }
                //foreach (var group in this.Products.OrderByDescending(t => t.Products.Count))
                //    childNodes.Add(group);
                //foreach (var entry in this.Elements)
                //    childNodes.Add(entry);
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
        [JsonIgnore]
        private CommandHandler _openfolder;
        [JsonIgnore]
        public CommandHandler OpenFolderCommand
        {
            get
            {
                return _openfolder ?? (_openfolder = new CommandHandler(obj =>
                {
                    SharedModel.InvokeOpenFolderEvent(this);
                },
                (obj) => !string.IsNullOrEmpty(FolderTo)
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
                    ItemClickEvent?.Invoke();
                },
                (obj) => true
                ));
            }
        }
        public delegate void ItemClick();
        public event ItemClick ItemClickEvent;

        public override string ToString()
        {
            return Name;
        }
    }
}
