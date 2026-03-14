using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PowerChat.App.Models
{
    public partial class ChatMessage : INotifyPropertyChanged
    {
        public string User
        {
            get;
            set;
        }
        = string.Empty;

        private string _text = string.Empty;
        public string Text
        {
            get => _text;
            set
            {
                if (_text != value)
                {
                    _text = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime Timestamp
        {
            get;
            set;
        }

        public string FormattedTime => Timestamp.ToString("HH:mm");


        private string _status = string.Empty;
        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}