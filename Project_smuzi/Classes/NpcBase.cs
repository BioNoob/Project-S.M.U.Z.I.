using Newtonsoft.Json;
using Project_smuzi.Controls;
using Project_smuzi.Models;
using Project_smuzi.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Project_smuzi.Classes
{

    public class NpcBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public NpcBase()
        {
            Groups = new ObservableCollection<NpcSector>();
            _Workers = new ObservableCollection<NpcWorker>();
            NpcSector.Identificator = 1;
            NpcWorker.Identificator = 1;
            SharedModel.NewWorkerCreateEvent += SharedModel_NewWorkerCreate;
            SharedModel.WorkerRequestToDelete += SharedModel_WorkerRequestToDelete;
            SharedModel.WorkerRequestToEdit += SharedModel_WorkerRequestToEdit;
            SharedModel.GroupRequestToDelete += SharedModel_GroupRequestToDelete;
        }

        private void SharedModel_GroupRequestToDelete(NpcSector sector)
        {

            if (System.Windows.Forms.MessageBox.Show($"Действительно удалить группу {sector.SectorLabel} ?", "Подтверждение удаления",
                System.Windows.Forms.MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
            {
                foreach (var item in sector.SectorWorkers)
                {
                    Workers.Where(t => t.Sectors.Contains(sector.SectorId)).ToList().ForEach(t => t.Sectors.Remove(sector.SectorId));
                }
            }
            Groups.Remove(sector);
        }

        private void SharedModel_WorkerRequestToEdit(NpcWorker user)
        {
            NewUserControl nuc = new NewUserControl() { Mode = true, FIO = user.Name, IsAdm = user.IsAdmin, WindowStartupLocation = WindowStartupLocation.CenterScreen };
            if ((bool)nuc.ShowDialog())
            {
                var a = Workers.Where(t => t.WorkerId == user.WorkerId).FirstOrDefault();
                if (a != null)
                {
                    a.IsAdmin = nuc.IsAdm;
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
                        Groups.Where(t => t.SectorId == item).ToList().ForEach(t => t.SectorWorkers.Remove(user.WorkerId));
                    }
                }
                else
                    return;
            }
            _Workers.Remove(user);
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Workers"));
        }

        private void SharedModel_NewWorkerCreate(NpcWorker user)
        {
            var a = Groups.Where(t => t.SectorLabel == "Без группы").FirstOrDefault();
            _Workers.Add(user);
        }


        private ObservableCollection<NpcSector> _groups;
        public ObservableCollection<NpcSector> Groups { get => _groups; set { SetProperty(ref _groups, value); } }

        private ObservableCollection<NpcWorker> _workers;

        private ObservableCollection<NpcWorker> _Workers { get => _workers; set => SetProperty(ref _workers, value); }
        //public IEnumerable<NpcWorker> Workers
        //{
        //    get
        //    {
        //        // Using an iterator so that client code can't access the underlying list
        //        foreach (var child in _Workers)
        //        {
        //            yield return child;
        //        }
        //    }
        //}
        [JsonIgnore]
        public ObservableCollection<NpcWorker> Workers
        {
            get
            {
                return _Workers;
            }
            set
            {
                _Workers = value;
            }
        }
        public void AddWorker(NpcWorker worker)
        {
            worker.Owner = this;
            _Workers.Add(worker);
        }
        public void RemoveWorker(NpcWorker worker)
        {
            _Workers.Remove(worker);
        }

        public void SaveJson()
        {
            string t = JsonConvert.SerializeObject(this, Formatting.Indented);
            Settings.Default.NPC_DB_json = t;
            Settings.Default.Save();
        }
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
