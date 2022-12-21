using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace TinyGUI
{
    public partial class App
    {
        public static readonly string Version = "1.0.8.0";
        private static readonly string[] AvailableLocales = {"zh", "zh-hant", "en", "de-de"};

        protected override void OnStartup(StartupEventArgs e)
        {
            InitLanguage();
#if !DEBUG
            RegisterEvents();
#endif
            base.OnStartup(e);
        }

        private void RegisterEvents()
        {
            //Task线程内未捕获异常处理事件
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            //UI线程未捕获异常处理事件（UI主线程）
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;

            //非UI线程未捕获异常处理事件
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void InitLanguage()
        {
            switch (TinyGUI.Properties.Settings.Default.LanguageIndex)
            {
                case -1:
                    String lang = System.Globalization.CultureInfo.CurrentCulture.Name;
                    if (!string.IsNullOrEmpty(lang) && lang.ToLower().Contains("zh"))
                    {
                        Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AvailableLocales[0]);
                        TinyGUI.Properties.Settings.Default.LanguageIndex = 0;
                    }
                    else
                    {
                        Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AvailableLocales[2]);
                        TinyGUI.Properties.Settings.Default.LanguageIndex = 2;
                    }

                    break;
                case 0:
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AvailableLocales[0]);
                    break;
                case 1:
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AvailableLocales[1]);
                    break;
                case 2:
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AvailableLocales[2]);
                    break;
                case 3:
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AvailableLocales[3]);
                    break;
            }
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            try
            {
                if (e.Exception is Exception exception)
                {
                    HandleException(exception);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            finally
            {
                e.SetObserved();
            }
        }

        //非UI线程未捕获异常处理事件
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                if (e.ExceptionObject is Exception exception)
                {
                    HandleException(exception);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        //UI线程未捕获异常处理事件
        private static void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                HandleException(e.Exception);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            finally
            {
                e.Handled = true;
            }
        }

        private static void HandleException(Exception e)
        {
            MessageBox.Show(e.InnerException != null ? e.InnerException.Message : e.Message, "ERROR");
        }
    }
}