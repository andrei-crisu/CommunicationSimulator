using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComSimulatorApp.generalUtilities;
using ComSimulatorApp.caplGenEngine.caplTypes;
using ComSimulatorApp.caplGenEngine.caplSyntax;

namespace ComSimulatorApp.caplGenEngine
{
    public class CaplSyntaxComponents
    {
        public static string Comment(string commentContent)
        {
            string generatedComment = "";
            if(commentContent.Contains("\n"))
            {
                commentContent = commentContent.Replace("\n", "\n" + CaplSyntaxConstants.SINGLE_LINE_COMMENT);
            }
            generatedComment = CaplSyntaxConstants.SINGLE_LINE_COMMENT + commentContent;
            return generatedComment;
        }

        public static string MultilineComment(string multilineCommentContent)
        {
            string generatedMultilineComment;
            generatedMultilineComment = CaplSyntaxConstants.MULTILINE_COMMENT_START + multilineCommentContent+
                CaplSyntaxConstants.MULTILINE_COMMENT_END;

            return generatedMultilineComment;
        }

        public static string GenerateCodeBlock(string contentOfCodeBlock)
        {
            string generatedCodeBlock;
            generatedCodeBlock = CaplSyntaxConstants.CODE_BLOCK_START + contentOfCodeBlock +
                CaplSyntaxConstants.CODE_BLOCK_END;
            return generatedCodeBlock;
        }

        public static string OnKeyPressEventBlock(string blockContent,char key)
        {
            string onKeyPressBlockString = "";
            const string specificKeyWordsSeq = "on key \'";
            string simpleWriteInstruction = CaplSyntaxComponents.SimpleWriteText("Key: {" + key.ToString() + "} pressed!")+CaplSyntaxConstants.NEW_LINE;
            if(!char.IsLetterOrDigit(key)&& !StringUtilities.isSpecialCh(key,""))
            {
                onKeyPressBlockString = simpleWriteInstruction;
                return onKeyPressBlockString;
            }
            else
            {
                onKeyPressBlockString = CaplSyntaxConstants.NEW_LINE + specificKeyWordsSeq + key + "\' ";
                onKeyPressBlockString += CaplSyntaxComponents.GenerateCodeBlock(simpleWriteInstruction+blockContent);
            }

            return onKeyPressBlockString;
        }

        public static string simpleFunctionCallBlock(string functionName, string parameter)
        {
            string functionCallString = functionName + "(" + parameter + ")";
            return functionCallString;
        }
        public static string  sendMessageInstruction(MessageType message)
        {
            string sendMessageString = "";
            sendMessageString = CaplSyntaxConstants.TAB_STR + CaplSyntaxComponents.simpleFunctionCallBlock("output", message.varName) +
                CaplSyntaxConstants.END_OF_INSTRUCTION + CaplSyntaxConstants.NEW_LINE;

            return sendMessageString;
        }

        public static string sendMessageInstructionsBlock(List<MessageType> messages,string comment="")
        {
            string block="";
            if (messages != null && messages.Count > 0)
            {
                block = CaplSyntaxComponents.MultilineComment(CaplSyntaxConstants.NEW_LINE+comment+CaplSyntaxConstants.NEW_LINE)+ CaplSyntaxConstants.NEW_LINE;
                foreach (MessageType message in messages)
                {
                    block += CaplSyntaxComponents.sendMessageInstruction(message);
                }
            }
            return block;
        }

        public static string OnTimerEventBlock(string blockContent, string timerName)
        {
            string onTimerEventBlockString = "";
            const string specificKeyWordSeq = "on timer ";
            onTimerEventBlockString = CaplSyntaxConstants.NEW_LINE + specificKeyWordSeq + timerName;
            onTimerEventBlockString += CaplSyntaxComponents.GenerateCodeBlock(blockContent);

            return onTimerEventBlockString;
        }

        public static string OnMessageEventBlock(string blockContent,string message)
        {
            string onMessageEventBlockString = "";

            const string specificKeyWordSeq = "on message ";
            onMessageEventBlockString = CaplSyntaxConstants.NEW_LINE + specificKeyWordSeq + message;
            onMessageEventBlockString += CaplSyntaxComponents.GenerateCodeBlock(blockContent);
            return onMessageEventBlockString;
        }

        public static string EncodingBlock()
        {
            string encodingBlock = "/*@!Encoding:1252*/";
            return encodingBlock+=CaplSyntaxConstants.NEW_LINE;

        }

        public static string IncludesBlock(string content)
        {
            string includesBlockStr = "includes";
            includesBlockStr += CaplSyntaxConstants.CODE_BLOCK_START;
            includesBlockStr += content;
            includesBlockStr += CaplSyntaxConstants.CODE_BLOCK_END;
            return includesBlockStr;
        }

        public static string  VariablesBlock(string content)
        {
            string variablesBlockString = "";
            const string specificKeyWordSeq = "variables";
            variablesBlockString = CaplSyntaxConstants.NEW_LINE + specificKeyWordSeq;
            variablesBlockString += CaplSyntaxComponents.GenerateCodeBlock(content);

            return variablesBlockString;
        }

        public static string SimpleWriteMessageInstruction(string messageVarName,string format="%d",string writeMessage="Message id: ")
        {
            string writeMsgInstrutionString = CaplSyntaxConstants.NEW_LINE + CaplSyntaxConstants.TAB_STR+ "write(\"" + writeMessage +format
                + "\"," + messageVarName + ")" + CaplSyntaxConstants.END_OF_INSTRUCTION+CaplSyntaxConstants.NEW_LINE;

            return writeMsgInstrutionString;
        }

        public static  string SimpleWriteText(string text)
        {
           string writeTextInstruction = CaplSyntaxConstants.NEW_LINE + CaplSyntaxConstants.TAB_STR + "write(\"" + text
                + "\")" + CaplSyntaxConstants.END_OF_INSTRUCTION + CaplSyntaxConstants.NEW_LINE;

            return writeTextInstruction;
        }

        //set timer instruction
        public static string setMsTimerInstruction(MsTimerType timer ,string comment="")
        {
            string setTimerString = "";
            setTimerString= (CaplSyntaxConstants.TAB_STR + "setTimer(" + timer.MsTimerName + "," + timer.MsPeriod.ToString() +
                    ")" + CaplSyntaxConstants.END_OF_INSTRUCTION + CaplSyntaxConstants.NEW_LINE);
            return setTimerString;
        }
        public static string setMsTimersBlock(List<MsTimerType> msTimers, string comment = "Set timers(ms)")
        {
            string setMsTimersString =CaplSyntaxComponents.Comment(comment);
            setMsTimersString += CaplSyntaxConstants.NEW_LINE;
            //set all msTimers
            foreach (MsTimerType msTimer in msTimers)
            {
                setMsTimersString += CaplSyntaxComponents.setMsTimerInstruction(msTimer, "");
            }

            return setMsTimersString;

        }

        public static string setTimerBlock(List<TimerType> timers,string comment="Set timers(seconds)")
        {
            string setTimersString = CaplSyntaxComponents.Comment(comment);
            setTimersString += CaplSyntaxConstants.NEW_LINE;
            foreach(TimerType timer in timers)
            {
                setTimersString+= (CaplSyntaxConstants.TAB_STR + "setTimer(" + timer.TimerName + "," + timer.Period.ToString() +
                    ")" + CaplSyntaxConstants.END_OF_INSTRUCTION + CaplSyntaxConstants.NEW_LINE);
            }

            return setTimersString;
        }

        public static string OnStartBlock(List<MsTimerType> msTimerList,List<TimerType>timerList,string otherContent="",string comment="")
        {
            string onStartBlockStr =CaplSyntaxConstants.TAB_STR+ CaplSyntaxComponents.MultilineComment(comment);
            onStartBlockStr += CaplSyntaxConstants.NEW_LINE;
            onStartBlockStr += "on start";

            //other instructions 
            onStartBlockStr += CaplSyntaxConstants.CODE_BLOCK_START;
            onStartBlockStr += otherContent + CaplSyntaxConstants.NEW_LINE;

            //set all msTimers
            if (msTimerList.Count > 0)
            {
                onStartBlockStr += CaplSyntaxComponents.setMsTimersBlock(msTimerList);
                onStartBlockStr += CaplSyntaxConstants.NEW_LINE;
            }

            //set all Timers
            if (timerList != null)
            {
                if (timerList.Count > 0)
                {
                    onStartBlockStr += CaplSyntaxComponents.setTimerBlock(timerList);
                    onStartBlockStr += CaplSyntaxConstants.NEW_LINE;
                }
            }

            //end of the  on start block
            onStartBlockStr += CaplSyntaxConstants.CODE_BLOCK_END;

            return onStartBlockStr;

        }

        public static string setAllMessagesPayloadData(List<MessageType> messages,string arrayName= "payloadValues", 
            uint ArraySize=8,string typeString="byte")
        {
            string generatedString="";
            generatedString += CaplFunctionsBuilder.declareArray(typeString, arrayName, ArraySize);
            foreach(MessageType message in messages)
            {
                generatedString += (CaplSyntaxComponents.Comment("For message: " + message.varName)+CaplSyntaxConstants.NEW_LINE);
                generatedString += CaplFunctionsBuilder.setArrayValues(arrayName,message.getMessagePayload(),typeString);
                generatedString += CaplFunctionsBuilder.callFunctionInstruction(CaplFunctionsBuilder.CAPL_SET_PAYLOAD_FUNCTION,
                    message.varName, arrayName);
            }

            return generatedString;
        }

    }
}
