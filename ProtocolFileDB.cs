using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Websbor_PasswordRespondents
{
    class ProtocolFileDB
    {
        public static void ProtocolLoadFileToDB(List <string> protocolLoad)
        {
            using (StreamWriter writeLoadProtocol = new StreamWriter(Environment.CurrentDirectory + @"\LoadProtocol.txt"))
            {
                foreach (var result in protocolLoad)
                {
                    writeLoadProtocol.WriteLine(result);
                }
            }
        }
    }
}
