using Project_smuzi.Classes;
using System.ComponentModel;

namespace Project_smuzi.Models
{
    public class ProductViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Product _prod;
        public Product Prod { get => _prod; set { _prod = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Prod")); } }
            
        //public string Source { get=>Resource}
        public ProductViewModel()
        {
            SharedModel.OpenInfoEvent += SharedModel_OpenInfoEvent;
           // Resources.папка_16.
        }

        private void SharedModel_OpenInfoEvent(Product product)
        {
            Prod = product;
        }

        //public void FilterBase_Parent()
        //{
        //    //В продуктах все продукты где упоинается этот продукт
        //    //в элементах все элементы ....
        //    Filtered_parent = new DataBase();
        //    foreach (var prd in Prod.Contaiments_in)
        //    {
        //        var q = DB.Productes.Where(t => t.BaseId == prd).FirstOrDefault();
        //        if (q == null)
        //        {
        //            var z = DB.Elementes.Where(t => t.BaseId == prd).FirstOrDefault();
        //            z = z.Copy();
        //            Filtered_parent.Elementes.Add(z);
        //        }
        //        else
        //        {
        //            q = q.Copy();
        //            Filtered_parent.Productes.Add(q);
        //        }
        //    }
        //}
        //public void FilterBase_child()
        //{
        //    //В продуктах все продукты где упоинается этот продукт
        //    //в элементах все элементы ....
        //    Filtered_parent = new DataBase();
        //    foreach (var prd in Prod.Contaiment)
        //    {
        //        var q = DB.Productes.Where(t => t.BaseId == prd.Key).FirstOrDefault();
        //        if (q == null)
        //        {
        //            var z = DB.Elementes.Where(t => t.BaseId == prd.Key).FirstOrDefault();
        //            z = z.Copy();
        //            z.Count = prd.Value;
        //            Filtered_parent.Elementes.Add(z);
        //        }
        //        else
        //        {
        //            q = q.Copy();
        //            q.Count = prd.Value;
        //            Filtered_parent.Productes.Add(q);
        //        }
        //    }
        //}
    }
}
