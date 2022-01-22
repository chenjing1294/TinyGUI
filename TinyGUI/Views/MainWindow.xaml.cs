using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using TinifyAPI;
using TinyGUI.Properties;
using TinyGUI.ViewModels;

namespace TinyGUI.Views
{
    public partial class MainWindow
    {
        private readonly MainModel _mainModel;

        public MainWindow()
        {
            InitializeComponent();
            _mainModel = (MainModel) DataContext;
            if (string.IsNullOrEmpty(Settings.Default.Key))
            {
                _mainModel.KeyGridVisibility = Visibility.Visible;
            }
            else
            {
                _mainModel.DropBoxGridVisibility = Visibility.Visible;
            }
        }

        private void DropButton_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Multiselect = true,
                Filter = "(*.jpg,*.png,*.jpeg,*.webp)|*.jpg;*.png;*.jpeg;*.webp;",
                CheckFileExists = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                string[] fileNames = openFileDialog.FileNames;
                foreach (string fileName in fileNames)
                {
                    _mainModel.ImageModels.Add(new ImageModel()
                    {
                        Path = fileName,
                        Name = GetFileName(fileName),
                        OriFileSize = GetFileSize(fileName)
                    });
                }
            }

            if (_mainModel.ImageModels.Any())
            {
                _mainModel.DropBoxGridVisibility = Visibility.Collapsed;
                _mainModel.KeyGridVisibility = Visibility.Collapsed;
                _mainModel.ImageListGridVisibility = Visibility.Visible;
                Compress(_mainModel);
            }
        }

        private void UIElement_OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] paths = (string[]) e.Data.GetData(DataFormats.FileDrop);
                if (paths != null)
                {
                    foreach (string path in paths)
                    {
                        if (IsImage(path))
                        {
                            _mainModel.ImageModels.Add(new ImageModel()
                            {
                                Path = path,
                                Name = GetFileName(path),
                                OriFileSize = GetFileSize(path)
                            });
                        }
                    }
                }
            }

            if (_mainModel.ImageModels.Any())
            {
                _mainModel.DropBoxGridVisibility = Visibility.Collapsed;
                _mainModel.KeyGridVisibility = Visibility.Collapsed;
                _mainModel.ImageListGridVisibility = Visibility.Visible;
                Compress(_mainModel);
            }

            e.Handled = true;
        }


        #region Utils

        string GetFileName(string path)
        {
            return System.IO.Path.GetFileName(path);
        }

        long GetFileSize(string path)
        {
            long lSize = 0;
            if (File.Exists(path))
            {
                FileInfo fileInfo = new FileInfo(path);
                lSize = fileInfo.Length;
            }

            return lSize;
        }

        bool IsImage(string path)
        {
            string extension = Path.GetExtension(path).ToLower();
            if (extension.EndsWith("webp") || extension.EndsWith("jpg")
                                           || extension.EndsWith("jpeg")
                                           || extension.EndsWith("png"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool ByteToFile(byte[] byteArray, string fileName)
        {
            bool result = false;
            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    fs.Write(byteArray, 0, byteArray.Length);
                    result = true;
                }
            }
            catch
            {
                result = false;
            }

            return result;
        }

        private void Compress(MainModel mainModel)
        {
            Tinify.Key = Settings.Default.Key;

            Tinify.Client.SendProgress += (mark, progress, totalSize) =>
            {
                foreach (ImageModel imageModel in mainModel.ImageModels)
                {
                    if (imageModel.Path.Equals(mark))
                    {
                        imageModel.Type = 0;
                        imageModel.Progress = progress;
                        imageModel.Size = totalSize ?? 0;
                    }
                }
            };

            Tinify.Client.ReceiveProgress += (mark, progress, totalSize) =>
            {
                foreach (ImageModel imageModel in mainModel.ImageModels)
                {
                    if (imageModel.Path.Equals(mark))
                    {
                        imageModel.Type = 1;
                        imageModel.Progress = progress;
                        imageModel.Size = totalSize ?? 0;
                    }
                }
            };

            ThreadPool.SetMaxThreads(5, 5);
            foreach (ImageModel file in mainModel.ImageModels)
            {
                ThreadPool.QueueUserWorkItem((obj) =>
                {
                    if (obj is ImageModel imageModel)
                    {
                        var sourceData = File.ReadAllBytes(imageModel.Path);
                        Task<Source> source = Tinify.FromBuffer(sourceData, imageModel.Path);
                        ByteToFile(source.ToBuffer(imageModel.Path).Result,
                            Path.GetDirectoryName(imageModel.Path) + "\\" +
                            $"{Path.GetFileNameWithoutExtension(imageModel.Path)}({new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()}){Path.GetExtension(imageModel.Path)}");
                    }
                }, file);
            }
        }

        #endregion

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(KeyTextBox.Text))
            {
                Settings.Default.Key = KeyTextBox.Text;
                Settings.Default.Save();
                _mainModel.KeyGridVisibility = Visibility.Collapsed;
                _mainModel.ImageListGridVisibility = Visibility.Collapsed;
                _mainModel.DropBoxGridVisibility = Visibility.Visible;
            }
        }

        private void ResetApiKey_OnClick(object sender, RoutedEventArgs e)
        {
            _mainModel.ImageListGridVisibility = Visibility.Collapsed;
            _mainModel.DropBoxGridVisibility = Visibility.Collapsed;
            _mainModel.KeyGridVisibility = Visibility.Visible;
            KeyTextBox.Text = Settings.Default.Key;
            e.Handled = true;
        }

        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://tinify.cn/developers");
            e.Handled = true;
        }

        private void StartButton_OnClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/chenjing1294/TinyGUI");
            e.Handled = true;
        }

        private void VersionHyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.redisant.cn/#Family");
            e.Handled = true;
        }
    }
}