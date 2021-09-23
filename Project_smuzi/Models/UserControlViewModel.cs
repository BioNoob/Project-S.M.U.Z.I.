using Newtonsoft.Json;
using Project_smuzi.Classes;
using Project_smuzi.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Linq;
using GongSolutions.Wpf.DragDrop;
using System.Windows;
using Project_smuzi.Controls;
using System.Collections;

namespace Project_smuzi.Models
{
    public class UserControlViewModel : INotifyPropertyChanged, IDropTarget
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public SectorDDHandler DDHandler { get; set; } = new SectorDDHandler();
        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            NpcWorker sourceItem = dropInfo.Data as NpcWorker;
            NpcSector targetItem = dropInfo.TargetItem as NpcSector;

            if (sourceItem != null && targetItem != null)
            {
                if (targetItem.SectorWorkers.Where(t => t == sourceItem.WorkerId).Count() <= 0)
                {
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                    dropInfo.Effects = DragDropEffects.Copy;
                }
            }
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {

            NpcWorker sourceItem = dropInfo.Data as NpcWorker;
            NpcSector targetItem = dropInfo.TargetItem as NpcSector;
            if (targetItem.SectorWorkers.Where(t => t == sourceItem.WorkerId).Count() <= 0)
            {
                sourceItem.SetWorkerSectors(targetItem);
                targetItem.SetWorkerGroupByThis(sourceItem);
                //sourceItem.Sectors.Add(targetItem.SectorLabel);
                //targetItem.SectorWorkers.Add(sourceItem);
            }

        }



        public UserControlViewModel()
        {
            SharedModel.ReadDataDone += SharedModel_ReadDataDone;
            Npc_base = new NpcBase();
            FilteredWorkers = new ObservableCollection<NpcWorker>();
            Npc_base.AddWorker(new NpcWorker() { Name = "STAS0", IsAdmin = true });
            Npc_base.AddWorker(new NpcWorker() { Name = "Дударенко Светлана Николаевна", IsAdmin = false });
            Npc_base.AddWorker(new NpcWorker() { Name = "Соколенко Артем Сергеевич", IsAdmin = false });
            Npc_base.AddWorker(new NpcWorker() { Name = "STAS3", IsAdmin = true });
            Npc_base.AddWorker(new NpcWorker() { Name = "STAS4", IsAdmin = false });


            var tg = new NpcSector(Npc_base) { SectorLabel = "тест" };
            tg.SetWorkerGroupByThis(Npc_base.Workers.Take(3));
            Npc_base.Groups.Add(tg);

            tg = new NpcSector(Npc_base) { SectorLabel = "тест2" };
            Npc_base.Workers.ElementAt(2).SetWorkerSectors(tg);
            Npc_base.Workers.ElementAt(3).SetWorkerSectors(tg);
            Npc_base.Workers.ElementAt(4).SetWorkerSectors(tg);

            Npc_base.Groups.Add(tg);

            SharedModel.WorkerRequesFromGrouptToDelete += SharedModel_WorkerRequesFromGrouptToDelete;
            SearchUserText = string.Empty;
            DDHandler.SectorContentChangedEvent += DDHandler_SectorContentChangedEvent;
        }

        private void SelectedProductFromGroup_ItemClickEvent()
        {
            SelectedGroup.SectorProducts.Remove(SelectedProductFromGroup.BaseId);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedGroup"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedGRProducts"));
        }

        private void DDHandler_SectorContentChangedEvent()
        {
            //throw new NotImplementedException();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedGroup"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedGRProducts"));
        }

        private DataBase _db;
        public DataBase DB
        {
            get => _db;
            set
            {
                _db = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DB"));
            }
        }
        private NpcBase _npc_db;
        public NpcBase Npc_base
        {
            get => _npc_db;
            set
            {
                _npc_db = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NPC_base"));
            }
        }

        private NpcSector selectedGroup;
        public NpcSector SelectedGroup
        {
            get => selectedGroup;
            set
            {
                selectedGroup = value;
                DDHandler.Owner = SelectedGroup;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedGroup"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedGRProducts"));
            }
        }
        private NpcWorker selectedWorker;
        public NpcWorker SelectedWorker
        {
            get => selectedWorker;
            set { selectedWorker = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedWorker")); }
        }
        private Product selectedProductFromGroup;
        public Product SelectedProductFromGroup
        {
            get => selectedProductFromGroup;
            set
            {
                selectedProductFromGroup = value;
                if (value != null)
                    SelectedProductFromGroup.ItemClickEvent += SelectedProductFromGroup_ItemClickEvent;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedProductFromGroup"));
            }
        }
        public ObservableCollection<Product> SelectedGRProducts
        {
            get
            {
                if (DB != null)
                    return SelectedGroup.GetProductsOf(DB);
                else
                    return null;
            }
        }

        private string _currentAdmin;
        public string CurrentAdmin
        {
            get => _currentAdmin;
            set
            {
                _currentAdmin = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentAdmin"));
            }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                if (!string.IsNullOrEmpty(_searchText))
                {
                    var q = DB.Productes.Where(t => t.Name.Contains(_searchText) || t.Identification.Contains(_searchText));
                    DB.Selector = new ObservableCollection<Product>(q);
                }
                else
                    DB.Selector = DB.Productes;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SearchText"));
            }
        }

        private string _searchUserText;
        public string SearchUserText
        {
            get => _searchUserText;
            set
            {
                List<ObservableCollection<int>> q;
                _searchUserText = value;
                FilteredWorkers.Clear();
                q = Npc_base.Groups.Select(t => t.SectorWorkers).ToList();
                foreach (var item in q)
                {
                    foreach (var qq in q)
                    {
                        foreach (var qqq in qq)
                        {
                            var buf = Npc_base.Workers.Where(t => t.WorkerId == qqq).FirstOrDefault();
                            FilteredWorkers.Add(buf);
                        }
                    }
                }
                FilteredWorkers = new ObservableCollection<NpcWorker>(FilteredWorkers.Distinct());
                if (!string.IsNullOrEmpty(_searchUserText))
                {
                    FilteredWorkers = new ObservableCollection<NpcWorker>(FilteredWorkers.Where(t => t.Name.Contains(_searchUserText)));
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SearchText"));
            }
        }
        private void SharedModel_ReadDataDone()
        {
            DB = SharedModel.DB.Copy();

            //GLUSHILKA
            var z = DB.Productes.Select(t => t.BaseId).Take(20).ToList();
            foreach (var item in z)
            {
                Npc_base.Groups[0].SectorProducts.Add(item);
            }

        }

        private void SharedModel_WorkerRequesFromGrouptToDelete(NpcWorker user)
        {
            SelectedGroup.SectorWorkers.Remove(user.WorkerId);
            user.Sectors.Remove(SelectedGroup.SectorId);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedGroup"));
        }

        private ObservableCollection<NpcWorker> _filteredworkers;
        public ObservableCollection<NpcWorker> FilteredWorkers
        {
            get
            {
                return _filteredworkers;
            }
            set
            {
                _filteredworkers = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FilteredWorkers"));
            }
        }
        private CommandHandler _newgroupadd;
        public CommandHandler NewGroupAddCommand
        {
            get
            {
                return _newgroupadd ??= new CommandHandler(obj =>
                {
                    var a = obj as Window;
                    NewGroupControl ngc = new NewGroupControl
                    {
                        Owner = a,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    if ((bool)ngc.ShowDialog())
                        Npc_base.Groups.Add(new NpcSector(Npc_base) { SectorLabel = ngc.GroupName });
                },
                (obj) => true
                );
            }
        }


    }
}
