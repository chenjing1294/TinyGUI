using System;
using System.Globalization;
using System.Windows.Data;

namespace TinyGUI.Converters
{
    public class ProgressConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is long size && values[1] is int progress && values[2] is int type &&
                values[3] is long oriFileSize)
            {
                if (progress != 100)
                {
                    if (type == 0)
                    {
                        return
                            $"{progress}% - 上传中 - {Converter(size * (progress / 100.0))}/{Converter(size)}";
                    }
                    else
                    {
                        return
                            $"{progress}% - 下载中 - {Converter(size * (progress / 100.0))}/{Converter(size)}";
                    }
                }
                else
                {
                    if (type == 0)
                    {
                        return "上传完毕, 准备下载....";
                    }
                    else
                    {
                        return $"下载完成({Converter(size * 1.0)}/-{((oriFileSize - size) * 100 / oriFileSize)}%)";
                    }
                }
            }

            return null;
        }

        public static string Converter(double size)
        {
            if (size < 1024)
            {
                return $"{size:F0}B";
            }
            else if (size < Math.Pow(1024.0, 2))
            {
                return $"{(size / 1024):F1}KB";
            }
            else if (size < Math.Pow(1024.0, 3))
            {
                return $"{(size / 1024 / 1024):F1}MB";
            }
            else if (size < Math.Pow(1024.0, 4))
            {
                return $"{(size / 1024 / 1024 / 1024):F1}GB";
            }
            else
            {
                return "Infinity";
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}