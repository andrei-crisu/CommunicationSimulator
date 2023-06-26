using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace ComSimulatorApp.caplGenEngine.caplTypes
{
    public class TimerType:CaplDataType,INotifyPropertyChanged
    {
        private static UInt64 timerObjCounter = 0;
        //va fi utilizat pentru a limita 
        //numarul de variabile create
        public const UInt32 MAX_TIMER_NR = 10000;
        public const UInt32 DEFAULT_PERIOD = 100;
        public const string DEFAULT_NAME = "sec_timer_";
        //perioada ca numar intreg de secunde
        UInt32 period { get; set; }

        //lista cu mesajele care se trimit
        //atunci cand trece un interval de timp specificat
        List<MessageType> attachedMessagesToSend;

        public TimerType(string name, UInt32 periodVal)
        {
            timerObjCounter++;
            TimerName = name;
            Period = periodVal;
            VariableType = CaplSyntaxConstants.KEYWORD_TIMER;

            attachedMessagesToSend = new List<MessageType>();
        }

        public TimerType(UInt32 timerPeriod = 100)
        {
            timerObjCounter++;
            TimerName = DEFAULT_NAME + timerObjCounter.ToString();
            Period = timerPeriod;
            VariableType = CaplSyntaxConstants.KEYWORD_TIMER;

            attachedMessagesToSend = new List<MessageType>();

        }

        public string getDeclartion()
        {
            string declartion;
            declartion = CaplSyntaxConstants.TAB_STR + VariableType + " " + varName + CaplSyntaxConstants.END_OF_INSTRUCTION;
            return declartion;
        }

        public string TimerName
        {
            get { return varName; }
            set
            {
                if (varName != value)
                {
                    varName = value;
                    NotifyPropertyChanged(nameof(TimerName));
                }
            }
        }

        public UInt32 Period
        {
            get { return period; }
            set
            {
                if (period != value)
                {
                    period = value;
                    NotifyPropertyChanged(nameof(Period));
                }
            }
        }

        public string VariableType
        {
            get { return varType; }
            set
            {
                if (varType != value)
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


        //metoda care asoctiaza trimiterea unui mesaj 
        // cu timerul
        public bool attachMessageFunction(MessageType message)
        {
            bool succes = false;
            if(attachedMessagesToSend.Contains(message))
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

        public int attachMessagesFunction(List<MessageType> messages)
        {
            //atasaza la timer o lista de mesaje
            int howManyAttached = 0;
            bool attachedStatus = false;
            foreach (MessageType message in messages)
            {
                attachedStatus = attachMessageFunction(message);
                if(attachedStatus)
                {
                    howManyAttached++;
                }
            }
            return howManyAttached;
        }

        //detaseaza un mesaj din lista de mesaje ale timer-ului
        public bool detachMessageFunction(MessageType message)
        {
            //
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

        //detaseaza o lista de mesaje
        public int detachMessagesFunction(List<MessageType> messages)
        {

            
            int howManyDetached = 0;
            bool detachedStatus = false;
            foreach (MessageType message in messages)
            {
                detachedStatus = detachMessageFunction(message);
                if (detachedStatus)
                {
                    howManyDetached++;
                }
            }
            return howManyDetached;
        }

        //seteaza intraga lista de mesaje atasate cu una noua
        public bool setAttachedMessagesList(List<MessageType> messages)
        {
            bool succes = false;
            attachedMessagesToSend.Clear();
            if (messages != null)
            {
                int counter = attachMessagesFunction(messages);
                if (counter == messages.Count)
                {
                    succes = true;
                }
            }
            return succes;
        }

        //detaseaza intreaga lista de mesaje cu una noua
        public List<MessageType> getAttachedMessagesList()
        {
            return attachedMessagesToSend;
        }

    }
}
