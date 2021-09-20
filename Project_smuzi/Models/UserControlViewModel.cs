﻿using Newtonsoft.Json;
using Project_smuzi.Classes;
using Project_smuzi.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Linq;

namespace Project_smuzi.Models
{
    public class UserControlViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

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
        public UserControlViewModel()
        {
            SharedModel.ReadDataDone += SharedModel_ReadDataDone;
            Groups = new List<NpcSector>();
            Workers = new List<NpcWorker>();
            var tg = new NpcSector() { SectorLabel = "тест" };
            tg.SectorWorkers.Add(new NpcWorker() { Name = "STAS", IsAdmin = true });
            tg.SectorWorkers.Add(new NpcWorker() { Name = "Дударенко Светлана николаевна", IsAdmin = false });
            tg.SectorWorkers.Add(new NpcWorker() { Name = "Соколенко Артем Сергеевич", IsAdmin = false });
            tg.SectorWorkers.ForEach(t => t.Sectors.Add(tg.SectorLabel));
            tg.SectorWorkers.Where(t => t.Name == "Соколенко Артем Сергеевич").FirstOrDefault().Sectors.Add("тест2");
            Groups.Add(tg);
            tg = new NpcSector() { SectorLabel = "тест2" };
            tg.SectorWorkers.Add(new NpcWorker() { Name = "STAS3", IsAdmin = true });
            tg.SectorWorkers.Add(new NpcWorker() { Name = "STAS4", IsAdmin = false });
            tg.SectorWorkers.Add(new NpcWorker() { Name = "STAS5", IsAdmin = false });
            tg.SectorWorkers.ForEach(t=>t.Sectors.Add(tg.SectorLabel));
            Groups.Add(tg);

            SharedModel.NewWorkerCreateEvent += SharedModel_NewWorkerCreate;
            SearchUserText = string.Empty;

        }
        public NpcWorker SelectedWorker { get; set; }

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
                List<List<NpcWorker>> q;
                _searchUserText = value;
                Workers.Clear();
                q = Groups.Select(t => t.SectorWorkers).ToList();
                foreach (var item in q)
                {
                    Workers.AddRange(item);
                }
                Workers = Workers.Distinct().ToList();
                if (!string.IsNullOrEmpty(_searchUserText))
                {
                    Workers = Workers.Where(t => t.Name.Contains(_searchUserText)).ToList();
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SearchText"));
            }
        }
        private void SharedModel_ReadDataDone()
        {
            DB =  SharedModel.DB.Copy();
        }

        private void SharedModel_NewWorkerCreate(NpcWorker user)
        {
            var a = Groups.Where(t => t.SectorLabel == "Без группы").FirstOrDefault();
            a.SectorWorkers.Add(user);
        }

        private List<NpcSector> _groups;
        public List<NpcSector> Groups
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
        private List<NpcWorker> _workers;
        public List<NpcWorker> Workers
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
        public void SaveJson()
        {
            string t = JsonConvert.SerializeObject(this, Formatting.Indented);
            Settings.Default.NPC_DB_json = t;
            Settings.Default.Save();
        }
    }
}
