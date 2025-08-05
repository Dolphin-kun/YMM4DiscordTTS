using System.ComponentModel;

namespace YMM4DiscordTTS.Models
{
    public class DictionaryEntry : INotifyPropertyChanged
    {
        private string _before = "";
        public string Before { get => _before; set { if (_before != value) { _before = value; OnPropertyChanged(nameof(Before)); } } }

        private string _after = "";
        public string After { get => _after; set { if (_after != value) { _after = value; OnPropertyChanged(nameof(After)); } } }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
