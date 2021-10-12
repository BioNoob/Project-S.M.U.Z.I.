using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Project_smuzi.Models
{
    class InputBoxViewModel : INotifyPropertyChanged
    {
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

        private string input;
        private string request;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Input { get => input; set => SetProperty(ref input, value); }
        public string Request { get => request; set => SetProperty(ref request, value); }
    }
}
