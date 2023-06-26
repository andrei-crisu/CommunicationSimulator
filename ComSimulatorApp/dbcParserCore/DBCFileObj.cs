using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComSimulatorApp.dbcParserCore
{
    // clasa ce stocheaza in cadrul unor liste 
    // toate elementele extrase din fisierul DBC
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

        // se seteaza lista nodurilor
        public void setNodesList(List<Node> nodesList)
        {
            this.nodes = nodesList;
        }
        
        //se returneaza lista nodurilor
        public List<Node> getNodesList()
        {
            return this.nodes;
        }
        //se seteaza lista semnalelor
        public void setSignalsList(List<Signal> signalsList)
        {
            this.signals = signalsList;
        }

        //se obtine lista semnalelor
        public List<Signal> getSignalsList()
        {
            return this.signals;
        }

        //se seteaza lista mesajelor
        public void setMessagesList(List<Message> messagesList)
        {
            this.messages = messagesList;
        }

        //se obtine lista mesajelor
        public List<Message> getMessagesList()
        {
            return this.messages;
        }

        
        //va fi implementat
        //intr-o ulterioara imbunatatire
        // in acest moment in cazul in care se detecteaza ca informatia este corupta
        // se va abandona interpretarea
        public int cleanCorruptedObjects()
        {
            int corruptedObjectsCount = 0;



            return corruptedObjectsCount;

        }


       
    }



}
