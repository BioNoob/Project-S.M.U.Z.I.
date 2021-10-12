using Project_smuzi.Classes;
using Project_smuzi.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

namespace Project_smuzi.Models
{
    class SwitchSectionsModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public SwitchSectionsModel()
        {
            Sections = new Dictionary<int, string>(SharedModel.Sections_dic);
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

        private Dictionary<int,string> sections;

        public Dictionary<int, string> Sections { get => sections; set => SetProperty(ref sections, value); }

        private CommandHandler addSectionCommand;
        public ICommand AddSectionCommand => addSectionCommand ??= new CommandHandler(AddSection);

        private void AddSection(object commandParameter)
        {
            string h = new InputBox("Введите имя категории").ShowDialog_();
            if (!string.IsNullOrEmpty(h))
            {
                SharedModel.AddSection(h);
                Sections = new Dictionary<int, string>(SharedModel.Sections_dic);
            }
        }

        private CommandHandler deleteSectionCommand;
        public CommandHandler DeleteSectionCommand
        {
            get
            {
                return deleteSectionCommand ?? (deleteSectionCommand = new CommandHandler(DeleteSection,
                (obj) => SharedModel.CanRemove(SelectedSection.Key)
                ));
            }
        }

        private void DeleteSection(object commandParameter)
        {
            SharedModel.RemoveSection(SelectedSection.Key);
            Sections = new Dictionary<int, string>(SharedModel.Sections_dic);
        }

        private KeyValuePair<int,string> selectedSection;

        public KeyValuePair<int, string> SelectedSection { get => selectedSection; set => SetProperty(ref selectedSection, value); }
    }
}
