using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComSimulatorApp.dbcParserCore
{

    public class Node
    {
        private string nodeName { get; set; }

        public Node(string nodeName)
        {
            this.nodeName = nodeName;
        }

        public Node()
        {
            this.nodeName =ParserConstants.DEFAULT_ERR_PARSED_OBJECT;
        }

        public string getName()
        {
            return this.nodeName;
        }

        public void setName(string nodeName)
        {
            this.nodeName = nodeName;
        }

        public string nodeToString()
        {
            string nodeString = "@ NODE: ";
            nodeString += nodeName;
            nodeString += "; ";

            return nodeString;
        }
    }

   
}
