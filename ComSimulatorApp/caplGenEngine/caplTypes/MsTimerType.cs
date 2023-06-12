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
        //can be used to test if a list has reached 
        // this  max number
        public const UInt32 MAX_TIMER_NR = 10000;
        public const  UInt32 DEFAULT_PERIOD = 100;
        public const string DEFAULT_NAME = "ms_timer_";
        //period as integer number of ms
        UInt32 msPeriod { get; set; }

        //for moment here i will have a list of attached messages
        //it is not the most optimal way to implement this 
        //but it is the fastest
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

        //attach messages functionalities
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

        public int AttachMessagesFunction(List<MessageType> messages)
        {
            //counts how many messages ( from messages list)
            //have been attached
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

        public int DetachMessagesFunction(List<MessageType> messages)
        {

            //counts how many messages ( from attachedMessagesToSend list)
            //have been detached
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

        public List<MessageType> getAttachedMessagesList()
        {
            return attachedMessagesToSend;
        }

        public void clearAttachedMessagesList()
        {
            attachedMessagesToSend.Clear();
        }

    }
}
