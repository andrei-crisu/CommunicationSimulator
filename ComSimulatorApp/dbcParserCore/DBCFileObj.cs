using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComSimulatorApp.dbcParserCore
{
    public class DBCFileObj
    {
        public List<Node> nodes;
        public List<Signal> signals;
        public List<Message> messages;

        public DBCFileObj()
        {
            nodes = new List<Node>();
            signals = new List<Signal>();
            messages = new List<Message>();
        }

        public void setNodesList(List<Node> nodesList)
        {
            this.nodes = nodesList;
        }
        
        public List<Node> getNodesList()
        {
            return this.nodes;
        }

        public void setSignalsList(List<Signal> signalsList)
        {
            this.signals = signalsList;
        }

        public List<Signal> getSignalsList()
        {
            return this.signals;
        }

        public void setMessagesList(List<Message> messagesList)
        {
            this.messages = messagesList;
        }

        public List<Message> getMessagesList()
        {
            return this.messages;
        }

        
        //to be implemented
        public int cleanCorruptedObjects()
        {
            int corruptedObjectsCount = 0;



            return corruptedObjectsCount;

        }


       
    }



}
