using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
#pragma warning disable CS0067
namespace Project_smuzi.Classes
{
    public class DataBase : INotifyPropertyChanged
    {
        [JsonIgnore]
        private ObservableCollection<Product> productes;
        [JsonIgnore]
        private ObservableCollection<Element> elementes;

        public DataBase()
        {
            Productes = new ObservableCollection<Product>();
            Elementes = new ObservableCollection<Element>();
        }
        public ObservableCollection<Product> Productes { get => productes; set { productes = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Productes")); } }
        public ObservableCollection<Element> Elementes { get => elementes; set { elementes = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Elementes")); } }

        [JsonIgnore]
        public ObservableCollection<Product> HeavyProducts => new ObservableCollection<Product>(Productes.Where(t => t.Elements.Count > 0 && t.Products.Count > 0));
        [JsonIgnore]
        public ObservableCollection<int> DeepList => new ObservableCollection<int>(Productes.Select(t => t.DeepLevel).Distinct().OrderBy(t => t));
        [JsonIgnore]
        public ObservableCollection<Product> Selector { get => selector; set { selector = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Selector")); } }
        [JsonIgnore]
        private ObservableCollection<Product> selector;

        public event PropertyChangedEventHandler PropertyChanged;
        public void LoadFromContaiment()
        {
            foreach (var prd in Productes)
            {
                foreach (var item in prd.Contaiment)
                {
                    var q = Productes.Where(t => t.BaseId == item).FirstOrDefault();
                    if (q == null)
                        prd.Elements.Add(Elementes.Where(t => t.BaseId == item).FirstOrDefault());
                    else
                        prd.Products.Add(q);
                }
            }
        }
    }
}
