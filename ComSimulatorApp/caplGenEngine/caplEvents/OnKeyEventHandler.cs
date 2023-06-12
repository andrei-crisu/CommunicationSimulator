using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComSimulatorApp.caplGenEngine.caplTypes;

namespace ComSimulatorApp.caplGenEngine.caplEvents
{
    public class OnKeyEventHandler : ICaplEventHandler
    {
        public string eventName { get; set; }
        public char keySymbol{get;set;}

        public List<MessageType> messagesList;

        public OnKeyEventHandler(char key)
        {
            eventName = "on key ";
            keySymbol = key;
            messagesList = new List<MessageType>();
        }

    }
}
