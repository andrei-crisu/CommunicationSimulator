using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComSimulatorApp.dbcParserCore
{
    public class ParserConstants
    {
        public enum ParserMsgTypes
        {
            INFO=0,
            ERROR=1,
            STATUS=2,
            WARNING=3,
            OTHER=4

        }

        public const int MESSAGE_TOKEN_NUMBER = 5;
        public const int SIGNAL_TOKEN_NUMBER = 8;
        public const string DEFAULT_ERR_PARSED_OBJECT = "DEFAULT_NAME_ERR";
        //acest lucru asigura ca numarul nu este un cod folosit
        //si reprezinta o valoare de eroare
        //semnalul care contine aceasta valoare este un mesaj parsat incorect.
        public const uint BYTE_ORDER_ERR = 71717;
    }
}
