using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComSimulatorApp.dbcParserCore
{
   
    public class Signal
    {
        private string signalName;
        private uint startBit;
        private uint length;
        private uint byteOrder;
        private char sign;
        private float factor;
        private float offset;
        private float minValue;
        private float maxValue;
        private string signalUnit;
        private List<Node> receivingNodes { get; set; }

        public Signal(string signalName,uint startBit, uint length,uint byteOrder,char sign, float factor,
            float offset,float minValue,float maxValue,string signalUnit,List<Node> receivingNode)
        {
            this.signalName = signalName;
            this.startBit = startBit;
            this.length = length;
            this.byteOrder = byteOrder;
            this.sign = sign;
            this.factor = factor;
            this.offset = offset;
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.signalUnit = signalUnit;
            receivingNodes = new List<Node>(receivingNode);
        }

        public Signal()
        {
            this.signalName = ParserConstants.DEFAULT_ERR_PARSED_OBJECT;
            this.startBit = 0;
            this.length = 0;
            this.byteOrder =ParserConstants.BYTE_ORDER_ERR;
            this.sign = '?';
            this.factor = 0;
            this.offset = 0;
            this.minValue = 121;
            this.maxValue = -121;
            this.signalUnit = "";
            receivingNodes = new List<Node>();
        }

        public void setSingnalName(string signalName)
        {
            this.signalName = signalName;
        }

        public string getSignalName()
        {
            return this.signalName;
        }

        public void setStartBit(uint startBit)
        {
            this.startBit = startBit;
        }

        public uint getStartBit()
        {
            return this.startBit;
        }

        public void setLength(uint length)
        {
            this.length = length;
        }

        public uint getLength()
        {
            return this.length;
        }

        public void setByteOrder(uint byteOrder)
        {
            this.byteOrder = byteOrder;
        }

        public uint getByteOrder()
        {
            return this.byteOrder;
        }

        public void setSign(char sign)
        {
            this.sign = sign;
        }

        public char getSign()
        {
            return this.sign;
        }

        public void setFactor(float factor)
        {
            this.factor = factor;
        }

        public float getFactor()
        {
            return this.factor;
        }

        public void setOffset(float offset)
        {
            this.offset = offset;
        }

        public float getOffset()
        {
            return this.offset;
        }


        public void setMinValue(float minValue)
        {
            this.minValue = minValue;
        }

        public float getMinValue()
        {
            return this.minValue;
        }


        public void setMaxValue(float maxValue)
        {
            this.maxValue = maxValue;
        }

        public float getMaxValue()
        {
            return this.maxValue;
        }

        public void setSignalUnit(string signalUnit)
        {
            this.signalUnit = signalUnit;
        }

        public string getSignalUnit()
        {
            return this.signalUnit;
        }


        public void setReceivingNodes(List<Node>receivingNodes)
        {
            this.receivingNodes = receivingNodes;
        }

        public List<Node> getReceivingNodes()
        {
            return this.receivingNodes;
        }

        private string endiannessToString()
        {
            string endiannessStr= "";
            switch (byteOrder)
            {
                case (uint)ENDIANNESS.INTEL:
                    endiannessStr = byteOrder.ToString();
                    break;

                case (uint)ENDIANNESS.MOTOROLA:
                    endiannessStr = byteOrder.ToString();
                    break;

                default:
                    endiannessStr = ENDIANNESS.UNKNOWN.ToString();
                    break;

            }

            return endiannessStr;
        }

        private string signToString()
        {
            string signStr = "";
            switch (sign)
            {
                case (char)SIGN_VAL.SIGNED_VALUE:
                    signStr = sign.ToString();
                    break;

                case (char)SIGN_VAL.UNSIGNED_VALUE:
                    signStr = sign.ToString();
                    break;

                default:
                    signStr = "UNKNOWN";
                    break;

            }

            return signStr;
        }


        public string signalToString(string separatorStringFormat="\n",string offsetStringFormat="\t")
        {
            string signalString = " > SIGNAL ";
            signalString += "["  + signalName + "]: "+ separatorStringFormat;
            signalString += offsetStringFormat + "startBit: " + startBit + separatorStringFormat;
            signalString += offsetStringFormat + "byteOrder: " + this.endiannessToString() + separatorStringFormat;
            signalString += offsetStringFormat + "sign: " + signToString() + separatorStringFormat;
            signalString += offsetStringFormat + "length: " + length.ToString() + separatorStringFormat;
            signalString += offsetStringFormat + "factor: " + factor.ToString() + separatorStringFormat;
            signalString += offsetStringFormat + "offset: " + offset.ToString() + separatorStringFormat;
            signalString += offsetStringFormat + "min: " + minValue.ToString() + separatorStringFormat;
            signalString += offsetStringFormat + "max: " + maxValue.ToString() + separatorStringFormat;
            signalString += offsetStringFormat + "signal Unit: " + signalUnit + separatorStringFormat;

            string receivingNodeSstring = "";
            if (receivingNodes.Count == 1)
            {
                receivingNodeSstring = "Receiving node: ";
            }
            else
            {
                receivingNodeSstring = "Receiving nodes: ";

            }

            foreach (Node node in receivingNodes)
            {
                receivingNodeSstring += "{" +node.nodeToString()+ "};";
            }
            signalString += offsetStringFormat + receivingNodeSstring + separatorStringFormat;

            return signalString;

        }


    }
}
