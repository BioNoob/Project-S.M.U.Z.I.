using Newtonsoft.Json;
using Project_smuzi.Classes;
using Project_smuzi.Properties;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Project_smuzi.Models
{
    static class SharedModel
    {
        public static bool IsAdminMode { get; set; } = true;
        public static DataBase DB { get; set; }
        public static NpcBase DB_Workers { get; set; }

        public static NpcWorker CurrentUser { get; set; }

        public static Dictionary<int, string> Sections = new Dictionary<int, string>(8)
        {
            {5, "Документация"},
            {10, "Комплексы"},
            {15, "Сборочные единицы"},
            {20, "Детали"},
            {25, "Стандартные изделия"},
            {30, "Прочие изделия"},
            {35, "Материалы"},
            {40, "Комплекты"}
        };
        public static void AddSection(string label)
        {
            int idlast = Sections.Keys.Max();
            Sections.Add(idlast + 5, label);
        }
        public static bool CanRemove(int id)
        {
            if (id > 40)
                return true;
            else
                return false;
        }
        public static void RemoveSection(int key)
        {
            Sections.Remove(key);
        }
        public static string GetInterpritation(int id)
        {
            return Sections[id];
        }
        public static Dictionary<int, string> Sections_dic => Sections;


        public delegate void OpenInfo(Product product);
        public static event OpenInfo OpenInfoEvent;
        public static event OpenInfo OpenFolderEvent;


        public delegate void JobInfo();
        public static event JobInfo ReadDataStart;
        public static event JobInfo CloseEvent;
        public delegate void JobInfo_base(DataBase db);
        public static event JobInfo_base ReadDataDone;

        public delegate void LogInfo(string text);
        public static event LogInfo LogInfoSend;
        //public delegate void UserEvents(NpcWorker user);


        //public delegate void GroupEvents(NpcSector user);
        public static void InvokeLogSend(string db)
        {
            Debug.WriteLine(db);
            LogInfoSend?.Invoke(db);
        }

        public static void InvokeReadDataDone(DataBase db)
        {
            ReadDataDone?.Invoke(db);
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
        public static void LoadDataBase()
        {
            if (!string.IsNullOrEmpty(Settings.Default.DB_json))
            {
                DB = JsonConvert.DeserializeObject<DataBase>(Settings.Default.DB_json);
                //DB.LoadFromContaiment();
            }
            else
                DB = new DataBase();
            InvokeReadDataDone(DB);
        }
    }
}
