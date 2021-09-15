using System.Collections.Generic;
using System.Linq;

namespace Project_smuzi.Classes
{
    public class DataBase
    {
        public DataBase()
        {
            Productes = new List<Product>();
            Elementes = new List<Element>();
        }
        public List<Product> Productes { get; set; }
        public List<Element> Elementes { get; set; }

        public List<Product> HeavyProducts => Productes.Where(t => t.Elements.Count > 0 && t.Products.Count > 0).ToList();
    }
}
