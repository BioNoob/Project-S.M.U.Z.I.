using GongSolutions.Wpf.DragDrop;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
#pragma warning disable CS0067
namespace Project_smuzi.Classes
{
    public class SectorDDHandler : IDropTarget, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
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

        private NpcSector owner;
        public NpcSector Owner { get => owner; set => SetProperty(ref owner, value); }
        public delegate void OwnerChange();
        public event OwnerChange SectorContentChangedEvent;
        public SectorDDHandler(NpcSector a)
        {
            Owner = a;
        }
        public SectorDDHandler()
        {
        }
        /// <inheritdoc />
        public void DragOver(IDropInfo dropInfo)
        {
            // Call default DragOver method, cause most stuff should work by default
            //GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.DragOver(dropInfo);
            ObservableCollection<Product> targetItem = dropInfo.TargetCollection as ObservableCollection<Product>;

            var b = dropInfo.Data.GetType();
            if (b.IsGenericType)
            {
                List<object> prd_list = (List<object>)dropInfo.Data;
                if (prd_list != null)
                {
                    if (prd_list.Count > 0)
                    {
                        dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                        dropInfo.Effects = DragDropEffects.Copy;
                        return;
                    }
                }
            }
            Product prd = dropInfo.Data as Product;
            if (targetItem != null && prd != null)
            {
                if (targetItem.Contains(prd))
                {
                    return;
                }
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Copy;
            }
        }

        /// <inheritdoc />
        public void Drop(IDropInfo dropInfo)
        {
            // The default drop handler don't know how to set an item's group. You need to explicitly set the group on the dropped item like this.
            //GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.Drop(dropInfo);

            //// Now extract the dragged group items and set the new group (target)
            //ObservableCollection<Product> targetItem = dropInfo.TargetCollection as ObservableCollection<Product>;
            var b = dropInfo.Data.GetType();
            if (b.IsGenericType)
            {
                List<object> prd_list = (List<object>)dropInfo.Data;
                if (prd_list != null)
                {
                    if (prd_list.Count > 0)
                    {
                        foreach (var item in prd_list)
                        {
                            Product prd_ = item as Product;
                            if (prd_ != null)
                            {
                                if (!Owner.SectorProducts.Contains(prd_.BaseId))
                                {
                                    Owner.SectorProducts.Add(prd_.BaseId);
                                    SectorContentChangedEvent?.Invoke();

                                }
                            }
                        }
                        return;
                    }
                }
            }
            if (dropInfo.Data is Product prd)
            {
                Owner.SectorProducts.Add(prd.BaseId);
                SectorContentChangedEvent?.Invoke();
            }


            //targetItem.SectorProducts.Add(prd.BaseId);

            //var data = DefaultDropHandler.ExtractData(dropInfo.Data).OfType<Product>().ToList();
            //foreach (var groupedItem in data)
            //{
            //    this.SectorProducts.Add(groupedItem.BaseId);
            //    //groupedItem.Group = dropInfo.TargetGroup.Name.ToString();
            //}
            // Changing group data at runtime isn't handled well: force a refresh on the collection view.
            //if (dropInfo.TargetCollection is ICollectionView)
            //{
            //((ICollectionView)targetItem).Refresh();
            //}
        }


    }
}
