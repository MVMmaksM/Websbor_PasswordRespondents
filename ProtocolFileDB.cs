using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Websbor_PasswordRespondents
{
    class ProtocolFileDB
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public static string fileNameProtocol { get; } = "ProtocolLoad.txt";
        public static string pathProtocolFile { get; } = Environment.CurrentDirectory + "\\Protocols\\";

        public static void CreateDirectoryProtocol()
        {
            try
            {
                if (!Directory.Exists(pathProtocolFile))
                {
                    Directory.CreateDirectory(pathProtocolFile);
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                logger.Error(ex.Message + ex.StackTrace);
            }            
        }
        public static void ProtocolLoadFileToDB(List<string> protocolLoad)
        {
            using (StreamWriter writeLoadProtocol = new StreamWriter(pathProtocolFile + fileNameProtocol))
            {
                foreach (var result in protocolLoad)
                {
                    writeLoadProtocol.WriteLine(result);
                }
            }
        }
    }
}
