using Newtonsoft.Json;
using Project_smuzi.Classes;
using Project_smuzi.Properties;

namespace Project_smuzi.Models
{
    static class SharedModel
    {
        public static bool IsAdminMode { get; set; } = true;
        public static DataBase DB { get; set; }
        public static NpcBase DB_Workers { get; set; }

        public static NpcWorker CurrentUser { get; set; }

        public delegate void OpenInfo(Product product);
        public static event OpenInfo OpenInfoEvent;
        public static event OpenInfo OpenFolderEvent;


        public delegate void JobInfo();
        public static event JobInfo ReadDataStart;
        public static event JobInfo CloseEvent;
        public delegate void JobInfo_base(DataBase db);
        public static event JobInfo_base ReadDataDone;
        //public delegate void UserEvents(NpcWorker user);


        //public delegate void GroupEvents(NpcSector user);


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
