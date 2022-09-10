using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace TinyGUI
{
    public partial class App
    {
        public static readonly bool AppStore = false;
        public static readonly string Version = "1.0.6.0";

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
            string language = TinyGUI.Properties.Settings.Default.Language;
            if (language == null || (!language.Equals("en-US") && !language.Equals("zh-CN") && !language.Equals("zh-TW")))
            {
                String lang = System.Globalization.CultureInfo.CurrentCulture.Name;
                language = lang.ToLower().Contains("en") ? "en-US" : "zh-CN";
                TinyGUI.Properties.Settings.Default.Language = language;
            }

            Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
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
        private static void App_DispatcherUnhandledException(object sender,
            System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
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