using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace ComSimulatorApp.caplGenEngine.caplTypes
{
    public class MsTimerType:CaplDataType, INotifyPropertyChanged
    {

        private static UInt64 timerObjCounter = 0;
        // MAX_TIMER_NR poate fi utilizat ca indicator
        //pentru numarul maxim de variabile ce se doresc a fi create 
        public const UInt32 MAX_TIMER_NR = 10000;
        public const  UInt32 DEFAULT_PERIOD = 100;
        public const string DEFAULT_NAME = "ms_timer_";
        //perioada ca numar intreg de milisecunde
        UInt32 msPeriod { get; set; }

        //lista de mesaje atasate
        //in cadrul handler-ului de timer 
        //se vor genera instructiuni de trimitere de mesaje pentru
        //mesajele atasate
        List<MessageType> attachedMessagesToSend;

        public MsTimerType( string name, UInt32 periodVal)
        {
            timerObjCounter++;
            MsTimerName = name;
            MsPeriod = periodVal;
            VariableType = CaplSyntaxConstants.KEYWORD_MSTIMER;

            attachedMessagesToSend = new List<MessageType>();

        }

        public MsTimerType(UInt32 timerPeriod= DEFAULT_PERIOD)
        {
            timerObjCounter++;
            MsTimerName = DEFAULT_NAME + timerObjCounter.ToString();
            MsPeriod = timerPeriod;
            VariableType = CaplSyntaxConstants.KEYWORD_MSTIMER;

            attachedMessagesToSend = new List<MessageType>();

        }

        public string getDeclartion()
        {
            string declartion;
            declartion = CaplSyntaxConstants.TAB_STR + VariableType + " " + varName + CaplSyntaxConstants.END_OF_INSTRUCTION;
            return declartion;
        }

        public string MsTimerName
        {
            get { return varName; }
            set
            {
                if (varName != value)
                {
                    varName = value;
                    NotifyPropertyChanged(nameof(MsTimerName));
                }
            }
        }

        public UInt32 MsPeriod
        {
            get { return msPeriod; }
            set
            {
                if (msPeriod != value)
                {
                    msPeriod = value;
                    NotifyPropertyChanged(nameof(MsPeriod));
                }
            }
        }

        public string VariableType
        {
            get { return varType; }
            set
            {
                if(varType!=value)
                {
                    varType = value;
                    NotifyPropertyChanged(nameof(VariableType));
                }
            }
            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //atasarea unui mesaj
        public bool AttachMessageFunction(MessageType message)
        {
            bool succes;
            if (attachedMessagesToSend.Contains(message))
            {
                return false;
            }
            else
            {
                attachedMessagesToSend.Add(message);
                succes = true;
            }

            return succes;
        }

        //atasarea unei intregi liste de mesaje
        public int AttachMessagesFunction(List<MessageType> messages)
        {
            int howManyAttached = 0;
            bool attachedStatus;
            foreach (MessageType message in messages)
            {
                attachedStatus = AttachMessageFunction(message);
                if (attachedStatus)
                {
                    howManyAttached++;
                }
            }
            return howManyAttached;
        }

        //detasarea unui mesaj
        public bool DetachMessageFunction(MessageType message)
        {
            bool succes = false;
            if (attachedMessagesToSend.Contains(message))
            {
                return false;
            }
            else
            {
                attachedMessagesToSend.Remove(message);
                succes = true;
            }

            return succes;
        }

        //detasarea unei liste de mesaje
        public int DetachMessagesFunction(List<MessageType> messages)
        {
            int howManyDetached = 0;
            bool detachedStatus = false;
            foreach (MessageType message in messages)
            {
                detachedStatus = DetachMessageFunction(message);
                if (detachedStatus)
                {
                    howManyDetached++;
                }
            }
            return howManyDetached;
        }

        //setarea intregii liste de mesaje atasate cu o noua lista
        public bool SetAttachedMessagesList(List<MessageType> messages)
        {
            bool succes = false;
            attachedMessagesToSend.Clear();
            if (messages != null)
            {
                int counter = AttachMessagesFunction(messages);
                if (counter == messages.Count)
                {
                    succes = true;
                }
            }
            return succes;
        }

        //obtinerea listei de mesaje atasate
        public List<MessageType> getAttachedMessagesList()
        {
            return attachedMessagesToSend;
        }

        //stergerea intregii liste de mesaje atasate
        public void clearAttachedMessagesList()
        {
            attachedMessagesToSend.Clear();
        }

    }
}
