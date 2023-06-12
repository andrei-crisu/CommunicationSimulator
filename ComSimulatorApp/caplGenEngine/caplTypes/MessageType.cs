using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;
using ComSimulatorApp.dbcParserCore;
using ComSimulatorApp.generalUtilities;
using System.Globalization;

namespace ComSimulatorApp.caplGenEngine.caplTypes
{
    public class MessageType:CaplDataType,INotifyPropertyChanged
    {
        public const string DEFAULT_PREFIX = "msg_";
        public const char DEFAULT_KEY = '#';

        //the id of the message
        public uint canId { get; set; }

        //the name of the message
        public string messageName { get; set; }

        //message length in bytes
        private uint messageLength;

        //the node that sends the message
        private Node sendingNode { get; set; }

        //the signals from the message 
        public List<Signal> signals;
        
        //this is only used to generate on message events
        //only for selected messages
        public bool selected { get; set; }

        //this is the basic sending rule
        //the message will be sent on 
        //the press of that key
        private char onKey { get; set; }

        //
        private List<Byte> messagePayload;




        public MessageType(Message message,bool selected=true,char onKey=DEFAULT_KEY,Byte initPayloadVal=0x00)
        {
            setMessage(message);
            OnMessage = selected;
            OnKey = onKey;
            varType = CaplSyntaxConstants.KEYWORD_MESSAGE;
            varName = DEFAULT_PREFIX + MessageName;
            messagePayload = new List<Byte>();
            InitializeMessagePayload(initPayloadVal);

        }

        public MessageType(MessageType initMessage)
        {
            CanId = initMessage.CanId;
            MessageName = initMessage.MessageName;
            MessageLength = initMessage.MessageLength;
            sendingNode = new Node();
            SendingNode = initMessage.SendingNode;
            signals = new List<Signal>();
            foreach (Signal signal in initMessage.signals)
            {
                signals.Add(signal);
            }

            OnMessage = initMessage.OnMessage;
            OnKey = initMessage.OnKey;
            varType = initMessage.varType;
            varName = initMessage.varName;
            messagePayload = new List<Byte>(initMessage.getMessagePayload());
        }

        public string getDeclartion()
        {
            string declartion;
            declartion = CaplSyntaxConstants.TAB_STR+varType + " " + MessageName + "   " + varName+CaplSyntaxConstants.END_OF_INSTRUCTION;
            return declartion;
        }

        public void setMessage(Message message)
        {
            CanId = message.getCanId();
            MessageName = message.getMessageName();
            MessageLength = message.getMessageLength();
            sendingNode = new Node();
            SendingNode = message.getSendingNode().getName();
            signals = new List<Signal>();
            foreach(Signal signal in message.signals)
            {
                signals.Add(signal);
            }
        }

        public void InitializeMessagePayload(Byte initialValue=0x00)
        {
            try
            {
                messagePayload.Clear();
                for (int i=0;i<messageLength;i++)
                {
                    messagePayload.Add(initialValue);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Exception caught!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void setMessagePayload(List<Byte> payloadData)
        {
            try
            {
                if (payloadData != null)
                {
                    messagePayload.Clear();
                    foreach (Byte byteData in payloadData)
                    {
                        messagePayload.Add(byteData);
                    }

                    if (messagePayload.Count > messageLength)
                    {
                        int extraBytes = messagePayload.Count - (int)messageLength;
                        for (int i = 0; i < extraBytes; i++)
                        {
                            messagePayload.RemoveAt(messagePayload.Count - 1);
                        }
                    }

                    if (messagePayload.Count < messageLength)
                    {
                        int neededBytesNr = (int)MessageLength - messagePayload.Count;

                        for (int i = 0; i < neededBytesNr; i++)
                        {
                            messagePayload.Insert(0, 0x00);
                        }
                    }

                    NotifyPropertyChanged(nameof(MessagePayload));
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Exception caught!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }


        }

        public void setMessagePayload(string payload)
        {
            try
            {
                string[] payloadBytesSubstrings = payload.TrimStart().TrimEnd().Split(" ");
                messagePayload.Clear();
                foreach (string dataByteString in payloadBytesSubstrings)
                {
                    if (byte.TryParse(dataByteString, NumberStyles.HexNumber, null, out byte byteResult))
                    {
                        messagePayload.Add(byteResult);
                    }
                    else
                    {
                        messagePayload.Add(0x00);
                    }
                }

                if (messagePayload.Count > messageLength)
                {
                    int extraBytes = messagePayload.Count - (int)messageLength;
                    for(int i=0;i<extraBytes;i++)
                    {
                        messagePayload.RemoveAt(messagePayload.Count - 1);
                    }
                }

                if (messagePayload.Count < messageLength)
                {
                    int neededBytesNr = (int)MessageLength - messagePayload.Count;

                    for (int i = 0; i < neededBytesNr; i++)
                    {
                        messagePayload.Insert(0, 0x00);
                    }
                }

                NotifyPropertyChanged(nameof(MessagePayload));
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Exception caught!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }

        public string MessageName
        {
            get { return messageName; }
            set
            {
                if (messageName != value)
                {
                    messageName = value;
                    NotifyPropertyChanged(nameof(MessageName));
                }
            }
        }

        public uint CanId
        {
            get { return canId; }
            set
            {
                if (canId != value)
                {
                    canId = value;
                    NotifyPropertyChanged(nameof(CanId));
                }
            }
        }

        public uint MessageLength
        {
            get { return messageLength; }
            set
            {
                if(messageLength!=value)
                {
                    messageLength = value;
                    NotifyPropertyChanged(nameof(MessageLength));
                }
            }
        }


        public string SendingNode
        {
            get { return sendingNode.getName(); }
            set
            {
                if (sendingNode.getName() != value)
                {
                    sendingNode.setName(value);
                    NotifyPropertyChanged(nameof(SendingNode));
                }
            }
        }


        public bool OnMessage
        {
            get { return selected; }
            set
            {
                if (selected != value)
                {
                    selected = value;
                    NotifyPropertyChanged(nameof(OnMessage));
                }
            }
        }

        public char OnKey
        {
            get { return onKey; }
            set
            {
                if (onKey != value)
                {
                    onKey = value;
                    NotifyPropertyChanged(nameof(onKey));
                }
            }
        }

        public List<Byte> getMessagePayload()
        {
            return messagePayload;
        }

        public string MessagePayload
        {
            get { return StringUtilities.ByteListToHexaString(messagePayload); }

            /*
             * set
             * {
             * }
            */
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public string messageToString(string separatorStringFormat = "\n", string offsetStringFormat = "\t",
           string secondSeparator = "\n", string secondOffsetFormat = "\t")
        {
            string messageString = "# MESSAGE: ";
            messageString += "[" + messageName + "]: " + secondSeparator;
            messageString += secondOffsetFormat + "ID: " + canId.ToString() + secondSeparator;
            messageString += secondOffsetFormat + "Length: " + messageLength.ToString() + secondSeparator;
            messageString += secondOffsetFormat + "Sending node: " + sendingNode.nodeToString() + secondSeparator;
            messageString += secondOffsetFormat + "Content ( signnals): " + secondSeparator;
            foreach (Signal signal in signals)
            {
                messageString += "   ";
                messageString += signal.signalToString(separatorStringFormat, offsetStringFormat);
                messageString += "\n";
            }

            return messageString;
        }

    }
}
