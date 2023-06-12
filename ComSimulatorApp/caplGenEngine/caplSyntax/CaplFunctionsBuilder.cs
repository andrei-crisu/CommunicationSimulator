using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComSimulatorApp.caplGenEngine.caplSyntax
{
    public class CaplFunctionsBuilder
    {

        public const string CAPL_SET_PAYLOAD_FUNCTION= "setPayloadData";
        public static string CaplSetPayloadFunctionDefinition()
        {
            string functionAsString;
            string functionBodyString;

            functionAsString = CaplSyntaxConstants.NEW_LINE + CaplSyntaxComponents.Comment("Set payload function definition!")+
                CaplSyntaxConstants.NEW_LINE;
            functionAsString += "void setPayloadData( message *msg,byte payloadArray[])";
            functionAsString += CaplSyntaxConstants.NEW_LINE;

            functionBodyString = "";
            functionBodyString += (CaplSyntaxConstants.TAB_STR+"int payloadLen;" + CaplSyntaxConstants.NEW_LINE);
            functionBodyString += (CaplSyntaxConstants.TAB_STR + "byte i;" + CaplSyntaxConstants.NEW_LINE);
            functionBodyString += (CaplSyntaxConstants.TAB_STR + "payloadLen = msg.dlc;" + CaplSyntaxConstants.NEW_LINE);
            functionBodyString += (CaplSyntaxConstants.TAB_STR + "for (i = 0; i < payloadLen; i++)" + CaplSyntaxConstants.NEW_LINE);

            string tempString = CaplSyntaxConstants.TAB_STR+"msg.byte (i) = payloadArray[i];"+ CaplSyntaxConstants.NEW_LINE;
            functionBodyString += CaplSyntaxComponents.GenerateCodeBlock(tempString);
            functionAsString += CaplSyntaxComponents.GenerateCodeBlock(functionBodyString);
            functionAsString += CaplSyntaxConstants.NEW_LINE;

            return functionAsString;
        }

        public static string declareArray(string arrayType, string arrayName,uint arraySize)
        {
            string declartionString="";
            declartionString += CaplSyntaxConstants.NEW_LINE + CaplSyntaxConstants.TAB_STR+arrayType +" "+arrayName;
            declartionString+="["+arraySize.ToString()+"]";
            declartionString += CaplSyntaxConstants.END_OF_INSTRUCTION;
            declartionString += CaplSyntaxConstants.NEW_LINE;
            return declartionString;
        }

        public static string setArrayValue(string arrayName,uint index, string value)
        {
            string setValueString="";
            setValueString += (arrayName+"["+index.ToString()+"] = "+value+CaplSyntaxConstants.END_OF_INSTRUCTION+" ");

            return setValueString;
        }

        public static string setArrayValues(string  arrayName,List<Byte> values,string arrayType="byte")
        {
            string strResult = CaplSyntaxConstants.TAB_STR;

            for (int i = 0; i < values.Count; i++)
            {
                strResult += setArrayValue(arrayName, (uint)i, "0x"+values.ElementAt(i).ToString("X2"));
            }
            strResult += CaplSyntaxConstants.NEW_LINE;
            return strResult;
        }

        public static string callFunctionInstruction(string functionName,string msgVarName,string arrayName)
        {
            string functionCallString="";
            functionCallString += CaplSyntaxConstants.TAB_STR;
            functionCallString += (functionName + "(" + msgVarName + "," + arrayName + ")" + CaplSyntaxConstants.END_OF_INSTRUCTION+
                CaplSyntaxConstants.NEW_LINE+CaplSyntaxConstants.NEW_LINE);
            return functionCallString;
        }

    }
}
