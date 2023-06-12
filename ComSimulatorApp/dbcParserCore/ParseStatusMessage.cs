using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComSimulatorApp.dbcParserCore
{
    //aceasta clasa nu este parte din fisierul DBC
    //reprezinta de fapt o clasa care 
    //defineste mesaje generate de Parser in timpul parsarii
    
    public class ParseStatusMessage
    {
        public string parseMsgName;
        public ParserConstants.ParserMsgTypes msgType;
        public string generationTime;

        public ParseStatusMessage(string parseMsgName, ParserConstants.ParserMsgTypes msgType=ParserConstants.ParserMsgTypes.OTHER)
        {
            this.parseMsgName = parseMsgName;
            this.msgType = msgType;
            DateTime currentDateTime = DateTime.Now;
            this.generationTime=currentDateTime.ToString("yyyy/MM/dd HH:mm:ss");

        }


    }
}
