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
    //acest tip mosteneste tipul de baza CaplDataType
    //si este utilizat pentru variabilele mesaj din CAPL
    public class MessageType:CaplDataType,INotifyPropertyChanged
    {
        public const string DEFAULT_PREFIX = "msg_";
        public const char DEFAULT_KEY = '#';

        //id-ul mesajului
        public uint canId { get; set; }

        //numele mesajului
        public string messageName { get; set; }

        //dimensiunea mesajului in octeti
        private uint messageLength;

        //variabila ce defineste nodul care trimite mesajul
        private Node sendingNode { get; set; }

        //lista semnalelor din componenta campului de date al mesajului
        public List<Signal> signals;
        
        //campul selected indica daca se va genera sau nu 
        //eveniment on message pentru mesaj
        public bool selected { get; set; }

        //indica tasta la apasarea careia se va trimite mesajul
        private char onKey { get; set; }

        //campul de date sub forma unei liste de octeti
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

        //metoda ce returneaza instructiunea de declarare a unei variabile de tip message sub 
        //forma unui sir de caractere
        public string getDeclartion()
        {
            string declartion;
            declartion = CaplSyntaxConstants.TAB_STR+varType + " " + MessageName + "   " + varName+CaplSyntaxConstants.END_OF_INSTRUCTION;
            return declartion;
        }

        //methoda seteaza valoarea camurilor mesajului pe baza unei variabile
        //de tipul Message
        //se realizeaza astfel o compatibilitate intre aceasta clasa
        //si clasa Message din cadrul modulului dbcParserCore
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

        //metoda InitializeMessagePayload realizeaza initializarea campului de date din 
        //cadrul unui mesaj cu o valoare predefinita 
        // by deafault cu valoarea 0x00
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

        //seteaza campul payload cu un alt camp payload
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
        //seteaza campul payload cu informatia extrasa dintr-un sir de caractere
        //ce reprezinta un camp payload
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

        //proprietate pentru setarea si obtinerea numelui mesajului
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

        //proprietate pentru setarea si obtinerea id-ului mesajului
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

        //proprietate utilizata pentru setarea si obtinerea dimensiunii campului de date
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

        //proprietate pentru setare si obtinerea numelui nodului care trimite mesajul
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

        //proprietate pentru a seta si obtine campul care specifica
        //daca se va genera manipulatorul on message
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

        //proprietate pentru a seta si obtine campul care specifica
        //daca se va genera manipulatorul on key pentru trimiterea mesajului
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

        //se returneaza lisista ce contine octetii campului payload
        public List<Byte> getMessagePayload()
        {
            return messagePayload;
        }

        //se obtine sirul de caractere al reprezentarii in format hexazecimal pentru
        //campul payload
        public string MessagePayload
        {
            get { return StringUtilities.ByteListToHexaString(messagePayload); }

            /*
             * set
             * {
             * }
            */
        }

        //implementare metoda pentru a indica modificarea campurilor
        //utila in acutalizarea interfetei grafice
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // converteste informatiile de baza despre mesaj la sir de caractere
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
