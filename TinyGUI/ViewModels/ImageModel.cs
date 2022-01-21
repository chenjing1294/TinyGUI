using System.ComponentModel;

namespace TinyGUI.ViewModels
{
    public class ImageModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private string _name;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }


        private string _path;

        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                OnPropertyChanged(nameof(Path));
            }
        }

        private int _type = 0; //0正在上传，1正在下载

        public int Type
        {
            get => _type;
            set
            {
                _type = value;
                OnPropertyChanged(nameof(Type));
            }
        }

        private long _size; //上传或下载时的大小

        public long Size
        {
            get => _size;
            set
            {
                _size = value;
                OnPropertyChanged(nameof(Size));
            }
        }

        private long _oriFileSize; //原始文件大小

        public long OriFileSize
        {
            get => _oriFileSize;
            set
            {
                _oriFileSize = value;
                OnPropertyChanged(nameof(OriFileSize));
            }
        }

        private int _progress = 0;

        public int Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                OnPropertyChanged(nameof(Progress));
            }
        }

        private string _progressMsg;

        public string ProgressMsg
        {
            get => _progressMsg;
            set
            {
                _progressMsg = value;
                OnPropertyChanged(nameof(ProgressMsg));
            }
        }
    }
}