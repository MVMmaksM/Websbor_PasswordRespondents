using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Websbor_PasswordRespondents
{  
    public partial class App : Application
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        System.Threading.Mutex mutex;
        private void App_Startup(object sender, StartupEventArgs e)
        {
            logger.Info("[Запуск программы]");

            bool createdNew;
            string mutName = "Приложение";
            mutex = new System.Threading.Mutex(true, mutName, out createdNew);

            //if (!createdNew)
            //{
            //    MessageBox.Show("Приложение уже запущено!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            //    this.Shutdown();
            //}
        }
    }
}
