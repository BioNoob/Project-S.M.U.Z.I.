using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
#pragma warning disable CS0067
namespace Project_smuzi.Classes
{
    public class Product : Element, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<Product> products;
        private ObservableCollection<Element> elements;
        private ObservableCollection<int> contaiments_in;

        public ObservableCollection<Product> Products { get => products; set { products = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Productes")); } }
        public ObservableCollection<Element> Elements { get => elements; set { elements = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Elementes")); } }

        public ObservableCollection<int> Contaiments_in { get => contaiments_in; set { contaiments_in = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Contaiments_in")); } }

        public Product(string ident) : base(ident)
        {
            Elements = new ObservableCollection<Element>();
            Products = new ObservableCollection<Product>();
            Contaiments_in = new ObservableCollection<int>();
        }
        public Product() : base()
        {
            Elements = new ObservableCollection<Element>();
            Products = new ObservableCollection<Product>();
            Contaiments_in = new ObservableCollection<int>();
        }
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
