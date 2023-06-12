using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace ComSimulatorApp.dbcParserCore
{
    public class Message
    {
        //the id of the message
        private uint canId;
        //the name of the message
        private string messageName;
        //message length in bytes
        private  uint messageLength;
        //the node that sends the message
        private Node sendingNode;
        public List<Signal> signals;

        public Message(uint canId, string messageName, uint messageLength, Node sendingNode)
        {
            this.canId = canId;
            this.messageName = messageName;
            this.messageLength = messageLength;
            this.sendingNode = sendingNode;
            this.signals = new List<Signal>();
        }

        public Message()
        {
            this.canId = 0;
            this.messageName = ParserConstants.DEFAULT_ERR_PARSED_OBJECT;
            this.messageLength = 0;
            this.sendingNode = new Node(ParserConstants.DEFAULT_ERR_PARSED_OBJECT);
            this.signals = new List<Signal>();
        }

        public void setCanId(uint canId)
        {
            this.canId = canId;
        }

        public uint getCanId()
        {
            return this.canId;
        }

        public void setMessageName(string messageName)
        {
            this.messageName = messageName;
        }

        public string getMessageName()
        {
            return this.messageName;
        }

        public void setMessageLength(uint messageLength)
        {
            this.messageLength = messageLength;
        }

        public uint getMessageLength()
        {
            return this.messageLength;
        }

        public void setSendingNode(Node node)
        {
            this.sendingNode = node;
        }

        public Node getSendingNode()
        {
            return this.sendingNode;
        }

        public string messageToString(string separatorStringFormat = "\n", string offsetStringFormat = "\t",
            string secondSeparator="\n",string secondOffsetFormat="\t")
        {
            string messageString = "# MESSAGE: ";
            messageString += "[" + messageName + "]: " + secondSeparator;
            messageString += secondOffsetFormat + "ID: " + canId.ToString() + secondSeparator;
            messageString += secondOffsetFormat + "Length: " + messageLength.ToString() + secondSeparator;
            messageString += secondOffsetFormat + "Sending node: " + sendingNode.nodeToString() + secondSeparator;
            messageString += secondOffsetFormat + "Content ( signnals): " +  secondSeparator;
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
