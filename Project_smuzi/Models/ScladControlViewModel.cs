using Project_smuzi.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Linq;
using System.Collections.ObjectModel;

namespace Project_smuzi.Models
{
    public class ScladControlViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        public ScladControlViewModel()
        {
            SharedModel.ReadDataDone += SharedModel_ReadDataDone;

        }

        private void SharedModel_ReadDataDone()
        {
            DB = SharedModel.DB.Copy();
            DB.Elementes = new ObservableCollection<Element>(DB.Elementes.Where(t => t.Section_id != 5));
        }

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

    }
}
