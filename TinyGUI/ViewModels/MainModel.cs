using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TinyGUI.Commands;

namespace TinyGUI.ViewModels
{
    public class MainModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private ObservableCollection<ImageModel> _imageModels = new ObservableCollection<ImageModel>();

        public ObservableCollection<ImageModel> ImageModels
        {
            get => _imageModels;
            set
            {
                _imageModels = value;
                OnPropertyChanged(nameof(ImageModels));
            }
        }


        private Visibility _imageListGridVisibility = Visibility.Collapsed;

        public Visibility ImageListGridVisibility
        {
            get => _imageListGridVisibility;
            set
            {
                _imageListGridVisibility = value;
                OnPropertyChanged(nameof(ImageListGridVisibility));
            }
        }

        private Visibility _keyGridVisibility = Visibility.Collapsed;

        public Visibility KeyGridVisibility
        {
            get => _keyGridVisibility;
            set
            {
                _keyGridVisibility = value;
                OnPropertyChanged(nameof(KeyGridVisibility));
            }
        }

        private Visibility _dropBoxGridVisibility = Visibility.Collapsed;

        public Visibility DropBoxGridVisibility
        {
            get => _dropBoxGridVisibility;
            set
            {
                _dropBoxGridVisibility = value;
                OnPropertyChanged(nameof(DropBoxGridVisibility));
            }
        }

        private ICommand _deleteCommand;

        public ICommand DeleteCommand => _deleteCommand ??
                                         (_deleteCommand = new Command((parameter) =>
                                         {
                                             ImageModels.Remove(parameter as ImageModel);
                                             if (!ImageModels.Any())
                                             {
                                                 KeyGridVisibility = Visibility.Collapsed;
                                                 ImageListGridVisibility = Visibility.Collapsed;
                                                 DropBoxGridVisibility = Visibility.Visible;
                                             }
                                         }));


        private bool _replaceOriginalImage = false;

        public bool ReplaceOriginalImage
        {
            get => _replaceOriginalImage;
            set
            {
                _replaceOriginalImage = value;
                OnPropertyChanged(nameof(ReplaceOriginalImage));
            }
        }
    }
}