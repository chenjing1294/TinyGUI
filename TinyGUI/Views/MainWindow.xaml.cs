using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using Microsoft.Win32;
using TinifyAPI;
using TinyGUI.ViewModels;

namespace TinyGUI.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainModel _mainModel;

        public MainWindow()
        {
            InitializeComponent();
            _mainModel = (MainModel) DataContext;
            if (string.IsNullOrEmpty(TinyGUI.Properties.Settings.Default.Key))
            {
                _mainModel.SettingRadioButtonIsChecked = true;
                KeyTextBox?.Focus();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            TinyGUI.Properties.Settings.Default.Save();
        }

        private async void DropButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TinyGUI.Properties.Settings.Default.Key))
            {
                return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Multiselect = true,
                Filter = "(*.jpg,*.png,*.jpeg,*.webp)|*.jpg;*.png;*.jpeg;*.webp;",
                CheckFileExists = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                string[] imgPaths = openFileDialog.FileNames;
                await Start(imgPaths.ToList());
            }
        }

        private async void UIElement_OnDrop(object sender, DragEventArgs e)
        {
            if (string.IsNullOrEmpty(TinyGUI.Properties.Settings.Default.Key))
            {
                return;
            }

            List<string> imgPaths = new List<string>();
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] paths = (string[]) e.Data.GetData(DataFormats.FileDrop);
                if (paths != null)
                {
                    foreach (string path in paths)
                    {
                        if (IsImage(path))
                        {
                            imgPaths.Add(path);
                        }
                    }
                }
            }

            await Start(imgPaths);
        }

        private async Task Start(List<string> imgPaths)
        {
            if (imgPaths.Count > 0)
            {
                if (_mainModel.SmartCutRadioButtonIsChecked)
                {
                    uint width = 0;
                    uint height = 0;
                    if (uint.TryParse(_mainModel.ImageWidth, out uint result1))
                    {
                        width = result1;
                    }

                    if (uint.TryParse(_mainModel.ImageHeight, out uint result2))
                    {
                        height = result2;
                    }

                    if (_mainModel.ScaleRadioButtonIsChecked)
                    {
                        string type = "scale";
                        if (width != 0 || height != 0)
                        {
                            await Compress(imgPaths, type, width, height);
                        }
                    }
                    else if (_mainModel.FitRadioButtonIsChecked)
                    {
                        string type = "fit";
                        if (width != 0 && height != 0)
                        {
                            await Compress(imgPaths, type, width, height);
                        }
                    }
                    else if (_mainModel.CoverRadioButtonIsChecked)
                    {
                        string type = "cover";
                        if (width != 0 && height != 0)
                        {
                            await Compress(imgPaths, type, width, height);
                        }
                    }
                    else if (_mainModel.ThumbRadioButtonIsChecked)
                    {
                        string type = "thumb";
                        if (width != 0 && height != 0)
                        {
                            await Compress(imgPaths, type, width, height);
                        }
                    }
                }
                else
                {
                    await Compress(imgPaths);
                }
            }
        }

        private async Task Compress(List<string> imgPaths)
        {
            imgPaths.Sort();
            _mainModel.IsIndeterminate = true;
            if (string.IsNullOrEmpty(Tinify.Key))
            {
                Tinify.Key = TinyGUI.Properties.Settings.Default.Key;
            }

            int i = 0;
            foreach (string path in imgPaths)
            {
                var source = Tinify.FromFile(path);
                source = Preserve(source);
                string savePath = path;
                if (!TinyGUI.Properties.Settings.Default.ReplaceOriginalImage)
                {
                    string extension = Path.GetExtension(path).ToLower();
                    string fileName = Path.GetFileName(path);
                    string directoryName = Path.GetDirectoryName(path);
                    Debug.Assert(directoryName != null, nameof(directoryName) + " != null");
                    string newPath = Path.Combine(directoryName, $"{fileName.Substring(0, fileName.Length - extension.Length)}-{GetTimeStamp()}{extension}");
                    savePath = newPath;
                }

                await source.ToFile(savePath);
                i++;
                _mainModel.ProgressBarValue = (i + 0.0) / imgPaths.Count;
            }

            _mainModel.IsIndeterminate = false;
            _mainModel.ProgressBarValue = 0;
        }

        private async Task Compress(List<string> imgPaths, string type, uint width, uint height)
        {
            imgPaths.Sort();
            _mainModel.IsIndeterminate = true;
            if (string.IsNullOrEmpty(Tinify.Key))
            {
                Tinify.Key = TinyGUI.Properties.Settings.Default.Key;
            }

            int i = 0;
            foreach (string path in imgPaths)
            {
                var source = Tinify.FromFile(path);
                source = Preserve(source);
                string savePath = path;
                if (!TinyGUI.Properties.Settings.Default.ReplaceOriginalImage)
                {
                    string extension = Path.GetExtension(path).ToLower();
                    string fileName = Path.GetFileName(path);
                    string directoryName = Path.GetDirectoryName(path);
                    Debug.Assert(directoryName != null, nameof(directoryName) + " != null");
                    savePath = Path.Combine(directoryName, $"{fileName.Substring(0, fileName.Length - extension.Length)}-{GetTimeStamp()}{extension}");
                }

                switch (type)
                {
                    case "scale":
                    {
                        if (width > 0)
                        {
                            var resized = source.Resize(new
                            {
                                method = type,
                                width = width
                            });

                            await resized.ToFile(savePath);
                        }
                        else if (height > 0)
                        {
                            var resized = source.Resize(new
                            {
                                method = type,
                                height = height
                            });

                            await resized.ToFile(savePath);
                        }

                        break;
                    }

                    case "fit":
                    case "cover":
                    case "thumb":
                    {
                        var resized = source.Resize(new
                        {
                            method = type,
                            width = width,
                            height = height
                        });

                        await resized.ToFile(savePath);
                        break;
                    }
                }


                i++;
                _mainModel.ProgressBarValue = (i + 0.0) / imgPaths.Count;
            }

            _mainModel.IsIndeterminate = false;
            _mainModel.ProgressBarValue = 0;
        }

        //保留元数据
        private Task<Source> Preserve(Task<Source> source)
        {
            List<string> metas = new List<string>();
            if (TinyGUI.Properties.Settings.Default.MetaCopyright)
            {
                metas.Add("copyright");
            }

            if (TinyGUI.Properties.Settings.Default.MetaLocation)
            {
                metas.Add("location");
            }

            if (TinyGUI.Properties.Settings.Default.MetaCreationTime)
            {
                metas.Add("creation");
            }

            if (metas.Count > 0)
            {
                return source.Preserve(metas.ToArray());
            }

            return source;
        }

        private static long GetTimeStamp()
        {
            TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalMilliseconds);
        }

        private static bool IsImage(string path)
        {
            string extension = Path.GetExtension(path).ToLower();
            if (extension.EndsWith("webp") || extension.EndsWith("jpg")
                                           || extension.EndsWith("jpeg")
                                           || extension.EndsWith("png"))
            {
                return true;
            }

            return false;
        }

        private void VersionHyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/chenjing1294/TinyGUI");
        }

        private void TinifyHyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(TinyGUI.Properties.Resources.KeyUrl);
        }

        private void RedisantHyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(TinyGUI.Properties.Resources.Redisant);
        }
    }
}