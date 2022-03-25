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
using Windows.Services.Store;

namespace TinyGUI.Views
{
    public partial class MainWindow
    {
        private StoreContext _context;
        private StoreAppLicense _appLicense;
        private readonly MainModel _mainModel;

        public MainWindow()
        {
            InitializeComponent();
            if (TinyGUI.Properties.Settings.Default.Language == "zh-CN")
            {
                ChineseCheckBox.IsChecked = true;
            }
            else
            {
                EnglishCheckBox.IsChecked = true;
            }

            Replace.IsChecked = TinyGUI.Properties.Settings.Default.ReplaceOriginalImage;
            _mainModel = (MainModel) DataContext;
            if (string.IsNullOrEmpty(Settings.Default.Key))
            {
                _mainModel.KeyGridVisibility = Visibility.Visible;
            }
            else
            {
                _mainModel.DropBoxGridVisibility = Visibility.Visible;
            }

            if (TinyGUI.App.AppStore)
            {
                InitializeLicense();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Settings.Default.Save();
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
                _mainModel.SettingGridVisibility = Visibility.Collapsed;
                _mainModel.ImageListGridVisibility = Visibility.Visible;
                Compress(_mainModel);
            }
        }

        private void UIElement_OnDrop(object sender, DragEventArgs e)
        {
            e.Handled = true;
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
                _mainModel.SettingGridVisibility = Visibility.Collapsed;
                _mainModel.ImageListGridVisibility = Visibility.Visible;
                Compress(_mainModel);
            }
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

        private void ByteToFile(byte[] byteArray, string fileName, bool isReplace = false)
        {
            using (FileStream fs = new FileStream(fileName, isReplace ? FileMode.Truncate : FileMode.OpenOrCreate, FileAccess.Write))
            {
                fs.Write(byteArray, 0, byteArray.Length);
            }
        }

        private void Compress(MainModel mainModel)
        {
            Tinify.Key = Settings.Default.Key;

            Tinify.Client.SendProgress += (mark, progress, totalSize) =>
            {
                foreach (ImageModel imageModel in mainModel.ImageModels)
                {
                    string m = System.Web.HttpUtility.UrlEncode(imageModel.Path);
                    if (m != null && m.Equals(mark))
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
                    string m = System.Web.HttpUtility.UrlEncode(imageModel.Path);
                    if (m != null && m.Equals(mark))
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
                        try
                        {
                            var sourceData = File.ReadAllBytes(imageModel.Path);
                            string mark = System.Web.HttpUtility.UrlEncode(imageModel.Path);
                            Task<Source> source = Tinify.FromBuffer(sourceData, mark);
                            var newPath = TinyGUI.Properties.Settings.Default.ReplaceOriginalImage
                                ? imageModel.Path
                                : $"{Path.GetDirectoryName(imageModel.Path)}\\{Path.GetFileNameWithoutExtension(imageModel.Path)}({new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()}){Path.GetExtension(imageModel.Path)}";

                            ByteToFile(source.ToBuffer(mark).Result, newPath, TinyGUI.Properties.Settings.Default.ReplaceOriginalImage);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.InnerException != null ? e.InnerException.Message : e.Message, "ERROR");
                        }
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
                _mainModel.SettingGridVisibility = Visibility.Collapsed;
                _mainModel.DropBoxGridVisibility = Visibility.Visible;
            }
        }

        private void ResetApiKey_OnClick(object sender, RoutedEventArgs e)
        {
            _mainModel.ImageListGridVisibility = Visibility.Collapsed;
            _mainModel.DropBoxGridVisibility = Visibility.Collapsed;
            _mainModel.SettingGridVisibility = Visibility.Collapsed;
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

        private void SettingButton_OnClick(object sender, RoutedEventArgs e)
        {
            _mainModel.KeyGridVisibility = Visibility.Collapsed;
            _mainModel.DropBoxGridVisibility = Visibility.Collapsed;
            _mainModel.ImageListGridVisibility = Visibility.Collapsed;
            _mainModel.SettingGridVisibility = Visibility.Visible;
            e.Handled = true;
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            TinyGUI.Properties.Settings.Default.Language = "zh-CN";
            if (ChineseCheckBox.IsChecked == true)
            {
                TinyGUI.Properties.Settings.Default.Language = "zh-CN";
            }
            else if (EnglishCheckBox.IsChecked == true)
            {
                TinyGUI.Properties.Settings.Default.Language = "en-US";
            }

            TinyGUI.Properties.Settings.Default.ReplaceOriginalImage = false || Replace.IsChecked == true;
            _mainModel.KeyGridVisibility = Visibility.Collapsed;
            _mainModel.ImageListGridVisibility = Visibility.Collapsed;
            _mainModel.SettingGridVisibility = Visibility.Collapsed;
            _mainModel.DropBoxGridVisibility = Visibility.Visible;
        }

        private void VersionHyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.redisant.cn/#Family");
            e.Handled = true;
        }


        #region AppStore

        private async void InitializeLicense()
        {
            if (_context == null)
            {
                _context = StoreContext.GetDefault();
            }

            _appLicense = await _context.GetAppLicenseAsync();
            if (_appLicense.IsActive)
            {
                if (_appLicense.IsTrial)
                {
                    _mainModel.Trial = $"Trial version. Expiration at: {DateTimeOffset.Now:yy-MM-dd}";
                }
                else
                {
                    _mainModel.Trial = string.Empty;

                    // Show the features that are available only with a full license.
                }
            }

            // Register for the licenced changed event.
            _context.OfflineLicensesChanged += context_OfflineLicensesChanged;
        }

        private async void context_OfflineLicensesChanged(StoreContext sender, object args)
        {
            // Reload the license.
            _appLicense = await _context.GetAppLicenseAsync();
            if (_appLicense.IsActive)
            {
                if (_appLicense.IsTrial)
                {
                    _mainModel.Trial = $"Trial version. Expiration at: {DateTimeOffset.Now:yy-MM-dd}";
                }
                else
                {
                    _mainModel.Trial = string.Empty;
                    // Show the features that are available only with a full license.
                }
            }
        }

        #endregion
    }
}