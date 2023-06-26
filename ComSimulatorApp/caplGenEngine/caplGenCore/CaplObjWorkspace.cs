using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComSimulatorApp.caplGenEngine.caplTypes;
using ComSimulatorApp.caplGenEngine.caplEvents;

namespace ComSimulatorApp.caplGenEngine
{
    //clasa contine variabilele globale utilizate in cadrul fisierului CAPL
    public class CaplObjWorkspace
    {
        public List<MsTimerType> msTimerList;
        public List<MessageType> messagesList;
        public List<OnKeyEventHandler> onKeyEvents;

        public List<int> integersList;

        public CaplObjWorkspace()
        {
            msTimerList = new List<MsTimerType>();
            messagesList = new List<MessageType>();
            onKeyEvents = new List<OnKeyEventHandler>();

            integersList = new List<int>();
        }

        public CaplObjWorkspace(List<MsTimerType> msTimers,List<MessageType> messages,List<OnKeyEventHandler> keEvents,
            List<int> integerVariables)
        {
            msTimerList = new List<MsTimerType>();
            messagesList = new List<MessageType>();
            onKeyEvents = new List<OnKeyEventHandler>();
            integersList = new List<int>();

            setMsTimerList(msTimerList);
            setMsgDataList(messages);
            setOnKeyEvents(keEvents);
            setIntegerList(integerVariables);
        }

        public void setMsTimerList(List<MsTimerType> list)
        {
            if (list != null)
                msTimerList = list.ToList<MsTimerType>();
        }

        public void setMsgDataList(List<MessageType> list)
        {
            messagesList.Clear();
            if(list!=null)
            {
                foreach (MessageType message in list)
                {
                    messagesList.Add(message);
                    
                }
            }
        }

        public void setOnKeyEvents(List<OnKeyEventHandler> eventsItems)
        {
            onKeyEvents.Clear();
            if(eventsItems != null)
            {
                foreach(OnKeyEventHandler eventItem in eventsItems)
                {
                    onKeyEvents.Add(eventItem);
                }
            }
        }

        public void setIntegerList(List<int> list)
        {
            integersList = list.ToList<int>();
        }

    }
}
