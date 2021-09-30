using Project_smuzi.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Project_smuzi.Models
{
    public class ScladControlViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        public ScladControlViewModel()
        {
            SharedModel.ReadDataDone += SharedModel_ReadDataDone;
            DB = new DataBase();
        }
        private Element selectedelement;
        public Element SelectedElement { get => selectedelement; set => SetProperty(ref selectedelement, value); }
        public ObservableCollection<Element> Elements { get => DB.Elementes; }
        private void SharedModel_ReadDataDone(DataBase db)
        {
            DB = db.Copy();
            DB.Elementes = new ObservableCollection<Element>(DB.Elementes.Where(t => t.Section_id != 5));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Elements"));
        }
        public object SelectedVal { get; set; }
        private DataBase _db;
        public DataBase DB { get => _db; set => SetProperty(ref _db, value); }




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

        private CommandHandler openFileCommand;
        public ICommand OpenFileCommand => openFileCommand ??= new CommandHandler(OpenFile);

        private void OpenFile(object commandParameter)
        {
            var t = commandParameter;
        }
    }
}
