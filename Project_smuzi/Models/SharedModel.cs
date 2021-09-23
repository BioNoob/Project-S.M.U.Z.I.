﻿using Newtonsoft.Json;
using Project_smuzi.Classes;
using Project_smuzi.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Project_smuzi.Models
{
    static class SharedModel
    {
        public static bool IsAdminMode { get; set; } = true;
        public static DataBase DB { get; set; }

        public delegate void OpenInfo(Product product);
        public static event OpenInfo OpenInfoEvent;
        public static event OpenInfo OpenFolderEvent;


        public delegate void JobInfo();
        public static event JobInfo ReadDataStart;
        public static event JobInfo ReadDataDone;
        public static event JobInfo CloseEvent;

        public delegate void UserEvents(NpcWorker user);
        public static event UserEvents NewWorkerCreateEvent;
        public static event UserEvents WorkerRequesFromGrouptToDelete;
        public static event UserEvents WorkerRequestToDelete;
        public static event UserEvents WorkerRequestToEdit;

        public delegate void GroupEvents(NpcSector user);
        public static event GroupEvents GroupRequestToDelete;

        public static void InvokeReadDataDone()
        {
            ReadDataDone?.Invoke();
        }
        public static void InvokeCloseApp()
        {
            CloseEvent?.Invoke();
        }
        public static void InvokeReadDataStart()
        {
            ReadDataStart?.Invoke();
        }
        public static void InvokeOpenInfoEvent(Product prd)
        {
            OpenInfoEvent?.Invoke(prd);
        }
        public static void InvokeOpenFolderEvent(Product prd)
        {
            OpenFolderEvent?.Invoke(prd);
        }
        public static void InvokeNewWorkerCreate(NpcWorker user)
        {
            NewWorkerCreateEvent?.Invoke(user);
        }
        public static void InvokeWorkerDelete(NpcWorker user)
        {
            WorkerRequestToDelete?.Invoke(user);
        }
        public static void InvokeWorkerEdit(NpcWorker user)
        {
            WorkerRequestToEdit?.Invoke(user);
        }
        public static void InvokeWorkerDeleteFromGroup(NpcWorker user)
        {
            WorkerRequesFromGrouptToDelete?.Invoke(user);
        }
        public static void InvokeGroupDelete(NpcSector sector)
        {
            GroupRequestToDelete?.Invoke(sector);
        }
        public static void LoadDataBase()
        {
            if (!string.IsNullOrEmpty(Settings.Default.DB_json))
            {
                DB = JsonConvert.DeserializeObject<DataBase>(Settings.Default.DB_json);
                DB.LoadFromContaiment();
            }
            DB.Selector = DB.Productes;
            InvokeReadDataDone();
        }
    }
}
