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
                if (targetItem.SectorWorkers.Where(t => t.Name == sourceItem.Name).Count() <= 0)
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
            if (targetItem.SectorWorkers.Where(t => t.Name == sourceItem.Name).Count() <= 0)
            {
                sourceItem.Sectors.Add(targetItem.SectorLabel);
                targetItem.SectorWorkers.Add(sourceItem);
            }

        }



        public UserControlViewModel()
        {
            SharedModel.ReadDataDone += SharedModel_ReadDataDone;
            Groups = new ObservableCollection<NpcSector>();
            Workers = new ObservableCollection<NpcWorker>();
            Workers.Add(new NpcWorker() { Name = "STAS", IsAdmin = true });
            Workers.Add(new NpcWorker() { Name = "Дударенко Светлана Николаевна", IsAdmin = false });
            Workers.Add(new NpcWorker() { Name = "Соколенко Артем Сергеевич", IsAdmin = false });
            Workers.Add(new NpcWorker() { Name = "STAS3", IsAdmin = true });
            Workers.Add(new NpcWorker() { Name = "STAS4", IsAdmin = false });


            var tg = new NpcSector() { SectorLabel = "тест" };
            tg.SectorWorkers.Add(.WorkerId);
            tg.SectorWorkers.Add(.WorkerId);
            tg.SectorWorkers.Add(.WorkerId);
            tg.SectorWorkers.ToList().ForEach(t => t.Sectors.Add(tg.SectorLabel));
            Groups.Add(tg);
            tg = new NpcSector() { SectorLabel = "тест2" };
            tg.SectorWorkers.Add(.WorkerId);
            tg.SectorWorkers.Add(.WorkerId);
            tg.SectorWorkers.Add(.WorkerId);
            tg.SectorWorkers.ToList().ForEach(t => t.Sectors.Add(tg.SectorLabel));
            Groups.Add(tg);

            SharedModel.NewWorkerCreateEvent += SharedModel_NewWorkerCreate;
            SharedModel.WorkerRequesFromGrouptToDelete += SharedModel_WorkerRequesFromGrouptToDelete;
            SharedModel.WorkerRequestToDelete += SharedModel_WorkerRequestToDelete;
            SharedModel.WorkerRequestToEdit += SharedModel_WorkerRequestToEdit;
            SharedModel.GroupRequestToDelete += SharedModel_GroupRequestToDelete;
            SearchUserText = string.Empty;
            DDHandler.SectorContentChangedEvent += DDHandler_SectorContentChangedEvent;
        }

        private void SharedModel_GroupRequestToDelete(NpcSector sector)
        {

            if (System.Windows.Forms.MessageBox.Show($"Действительно удалить группу {sector.SectorLabel} ?", "Подтверждение удаления",
                System.Windows.Forms.MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
            {
                foreach (var item in sector.SectorWorkers)
                {
                    Workers.Where(t => t.Sectors.Contains(sector.SectorLabel)).ToList().ForEach(t => t.Sectors.Remove(sector.SectorLabel));
                }
            }
            Groups.Remove(sector);
        }

        private void SharedModel_WorkerRequestToEdit(NpcWorker user)
        {
            NewUserControl nuc = new NewUserControl() { Mode = true, FIO = user.Name, isAdm = user.IsAdmin };
            if ((bool)nuc.ShowDialog())
            {
                var a = Workers.Where(t => t.WorkerId == user.WorkerId).FirstOrDefault();
                if (a != null)
                {
                    a.IsAdmin = nuc.isAdm;
                    a.Name = nuc.FIO;
                }
                //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Workers"));
            }
        }

        private void SharedModel_WorkerRequestToDelete(NpcWorker user)
        {
            if (user.Sectors.Count > 0)
            {
                if (System.Windows.Forms.MessageBox.Show($"Данный пользователь находится в группах:\n\t{string.Join("\n\t", user.Sectors)}\nУдалить пользователя?", "Внимание!",
                    System.Windows.Forms.MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
                {
                    foreach (var item in user.Sectors)
                    {
                        Groups.Where(t => t.SectorLabel == item).ToList().ForEach(t => t.SectorWorkers.Remove(user));
                    }
                }
                else
                    return;
            }
            Workers.Remove(user);
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Workers"));
        }

        private void SharedModel_WorkerRequesFromGrouptToDelete(NpcWorker user)
        {
            SelectedGroup.SectorWorkers.Remove(user);
            user.Sectors.Remove(SelectedGroup.SectorLabel);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedGroup"));
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
                List<ObservableCollection<NpcWorker>> q;
                _searchUserText = value;
                Workers.Clear();
                q = Groups.Select(t => t.SectorWorkers).ToList();
                foreach (var item in q)
                {
                    foreach (var qq in q)
                    {
                        foreach (var qqq in qq)
                        {
                            Workers.Add(qqq);
                        }
                    }
                }
                Workers = new ObservableCollection<NpcWorker>(Workers.Distinct());
                if (!string.IsNullOrEmpty(_searchUserText))
                {
                    Workers = new ObservableCollection<NpcWorker>(Workers.Where(t => t.Name.Contains(_searchUserText)));
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
                Groups[0].SectorProducts.Add(item);
            }

        }

        private void SharedModel_NewWorkerCreate(NpcWorker user)
        {
            var a = Groups.Where(t => t.SectorLabel == "Без группы").FirstOrDefault();
            Workers.Add(user);
            Workers = Workers;
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Workers"));
        }

        private ObservableCollection<NpcSector> _groups;
        public ObservableCollection<NpcSector> Groups
        {
            get
            {
                return _groups;
            }
            set
            {
                _groups = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Groups"));
            }
        }
        private ObservableCollection<NpcWorker> _workers;


        public ObservableCollection<NpcWorker> Workers
        {
            get
            {
                return _workers;
            }
            set
            {
                _workers = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Workers"));
            }
        }

        private CommandHandler _newgroupadd;
        public CommandHandler NewGroupAddCommand
        {
            get
            {
                return _newgroupadd ?? (_newgroupadd = new CommandHandler(obj =>
                {
                    var a = obj as Window;
                    NewGroupControl ngc = new NewGroupControl();
                    ngc.Owner = a;
                    ngc.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    if ((bool)ngc.ShowDialog())
                        Groups.Add(new NpcSector() { SectorLabel = ngc.GroupName });
                },
                (obj) => true
                ));
            }
        }

        public void SaveJson()
        {
            string t = JsonConvert.SerializeObject(this, Formatting.Indented);
            Settings.Default.NPC_DB_json = t;
            Settings.Default.Save();
        }
    }
}
