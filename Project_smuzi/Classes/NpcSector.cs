using GongSolutions.Wpf.DragDrop;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
#pragma warning disable CS0067
namespace Project_smuzi.Classes
{
    public class NpcSector : INotifyPropertyChanged, IDropTarget
    {
        /// <inheritdoc />
        public void DragOver(IDropInfo dropInfo)
        {
            // Call default DragOver method, cause most stuff should work by default
            GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.DragOver(dropInfo);
            if (dropInfo.TargetGroup == null)
            {
                dropInfo.Effects = System.Windows.DragDropEffects.None;
            }
        }

        /// <inheritdoc />
        public void Drop(IDropInfo dropInfo)
        {
            // The default drop handler don't know how to set an item's group. You need to explicitly set the group on the dropped item like this.
            //GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.Drop(dropInfo);

            //// Now extract the dragged group items and set the new group (target)
            //var data = DefaultDropHandler.ExtractData(dropInfo.Data).OfType<GroupedItem>().ToList();
            //foreach (var groupedItem in data)
            //{
            //    groupedItem.Group = dropInfo.TargetGroup.Name.ToString();
            //}

            //// Changing group data at runtime isn't handled well: force a refresh on the collection view.
            //if (dropInfo.TargetCollection is ICollectionView)
            //{
            //    ((ICollectionView)dropInfo.TargetCollection).Refresh();
            //}
        }
        public string SectorLabel { get; set; }
        public ObservableCollection<NpcWorker> SectorWorkers { get => sectorWorkers; set => SetProperty(ref sectorWorkers, value); }
        public ObservableCollection<int> SectorProducts { get => sectorProducts; set => SetProperty(ref sectorProducts, value); }
        public NpcSector()
        {
            SectorWorkers = new ObservableCollection<NpcWorker>();
            SectorProducts = new ObservableCollection<int>();
        }

        public ObservableCollection<Product> GetProductsOf(DataBase DB)
        {
            var q = new List<Product>();
            foreach (var item in SectorProducts)
            {
                q.Add(DB.Productes.Where(t => t.BaseId == item).FirstOrDefault());
            }
            return new ObservableCollection<Product>(q);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private ObservableCollection<NpcWorker> sectorWorkers;
        private ObservableCollection<int> sectorProducts;
        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, newValue))
            {
                field = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }
            return false;
        }




    }
}
