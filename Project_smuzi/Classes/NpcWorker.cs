using Newtonsoft.Json;
using Project_smuzi.Controls;
using Project_smuzi.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
#pragma warning disable CS0067
namespace Project_smuzi.Classes
{
    public class NpcWorker : INotifyPropertyChanged
    {
        public static int Identificator = 1;



        [JsonIgnore]
        public string GetImg
        {
            get
            {
                if (IsAdmin)
                    return "/Resources/админ_16.png";
                else
                    return "/Resources/юзер_16.png";
            }
        }
        private bool isAdmin;
        public bool IsAdmin { get => isAdmin; set => SetProperty(ref isAdmin, value); }
        private string name;
        public string Name { get => name; set => SetProperty(ref name, value); }
        private int workerId;
        public int WorkerId { get => workerId; set => SetProperty(ref workerId, value); }

        private ObservableCollection<int> sectors;
        public ObservableCollection<int> Sectors { get => sectors; set => SetProperty(ref sectors, value); }

        [JsonIgnore]
        public ObservableCollection<NpcSector> WorkerGroups
        {
            get
            {
                return new ObservableCollection<NpcSector>(SharedModel.DB_Workers.Groups.Where(t => t.SectorWorkers.Contains(WorkerId)));
            }
        }
        public NpcWorker()
        {
            Sectors = new ObservableCollection<int>();
            WorkerId = Identificator++;
            SharedModel.DB_Workers.PropertyChanged += DB_Workers_PropertyChanged;
        }

        private void DB_Workers_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Workers")
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("WorkerGroups"));
        }

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

        private CommandHandler deleteFromGroupCommand;
        public ICommand DeleteFromGroupCommand => deleteFromGroupCommand ??= new CommandHandler(DeleteFromGroup);

        private void DeleteFromGroup(object commandParameter)
        {
            SharedModel.DB_Workers.RemoveWorkerFromGroup(this, commandParameter as NpcSector);
        }

        private CommandHandler changeWorkerCommand;
        public ICommand ChangeWorkerCommand => changeWorkerCommand ??= new CommandHandler(ChangeWorker);

        private void ChangeWorker(object commandParameter)
        {

            var a = commandParameter as Window;
            NewUserControl nuc = new NewUserControl
            {
                Owner = a,
                Mode = true,
                FIO = this.Name,
                IsAdm = this.IsAdmin,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            if ((bool)nuc.ShowDialog())
            {
                this.IsAdmin = nuc.IsAdm;
                this.Name = nuc.FIO;
                SharedModel.DB_Workers.ChangeWorker(this);
            }

            //
        }
        private CommandHandler deleteWorkerCommand;
        public ICommand DeleteWorkerCommand => deleteWorkerCommand ??= new CommandHandler(DeleteWorker);

        private void DeleteWorker(object commandParameter)
        {
            if (System.Windows.Forms.MessageBox.Show($"Удалить пользователя \"{Name}\"?", "Удаление пользователя", System.Windows.Forms.MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
                SharedModel.DB_Workers.RemoveWorker(this);
        }
    }
}
