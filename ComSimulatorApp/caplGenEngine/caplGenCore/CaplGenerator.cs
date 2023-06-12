using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComSimulatorApp.caplGenEngine.caplTypes;
using ComSimulatorApp.caplGenEngine.caplEvents;
using ComSimulatorApp.caplGenEngine.caplSyntax;

namespace ComSimulatorApp.caplGenEngine
{
    public class CaplGenerator
    {

        public CaplObjWorkspace globalVariables;
        private string fileContent;


        public CaplGenerator(List<MessageType> messageList,List<MsTimerType> msTimerList, string initialComment="")
        {
            globalVariables = new CaplObjWorkspace();
            globalVariables.setMsgDataList(messageList);
            globalVariables.setMsTimerList(msTimerList);
            globalVariables.setOnKeyEvents(getUsedKeys(messageList));

            fileContent = CaplSyntaxComponents.EncodingBlock();
            string generationMoment = DateTime.Now.ToString("yyyy/MM/dd  [HH:mm:ss]");
            fileContent += CaplSyntaxComponents.MultilineComment(initialComment + CaplSyntaxConstants.NEW_LINE+"Generated: "+generationMoment+
                CaplSyntaxConstants.NEW_LINE);

            string onMsgBlock = GenerateOnMessageEvents(globalVariables.messagesList);
            string onTimerBlock = GenerateOnMsTimervents(globalVariables.msTimerList);
            string onKeyBlock = GenerateOnKeyEvents(globalVariables.onKeyEvents);

            fileContent += CaplSyntaxConstants.NEW_LINE;
            fileContent += generateVariablesBlock();
            fileContent += CaplFunctionsBuilder.CaplSetPayloadFunctionDefinition();

            string otherContentOnStartBlock = CaplSyntaxComponents.setAllMessagesPayloadData(globalVariables.messagesList);
            fileContent += CaplSyntaxComponents.OnStartBlock(msTimerList, null, otherContentOnStartBlock, "Initialize instructions!");

            if (onMsgBlock.Length > 0)
            {
                fileContent += (CaplSyntaxComponents.Comment("WHEN A MESSAGE IS RECEIVED EVENTS:") + CaplSyntaxConstants.NEW_LINE);
            }
            fileContent += onMsgBlock;

            if (onTimerBlock.Length > 0)
            {
                fileContent += (CaplSyntaxComponents.Comment("ON TIMER EVENTS:") + CaplSyntaxConstants.NEW_LINE);
            }
            fileContent += onTimerBlock;

            if (onKeyBlock.Length > 0)
            {
                fileContent += (CaplSyntaxComponents.Comment("WHEN A KEY IS PRESSED:") + CaplSyntaxConstants.NEW_LINE);
            }
            fileContent += onKeyBlock;

        }

        private string  GenerateOnMessageEvents(List<MessageType> itemsList)
        {
            string onMsgString="";
            foreach(MessageType item in itemsList)
            {
                if (item.OnMessage)
                {
                    string blockContent = CaplSyntaxComponents.SimpleWriteMessageInstruction(item.varName + ".id");
                    onMsgString += CaplSyntaxComponents.OnMessageEventBlock(blockContent, item.messageName);
                }
            }

            return onMsgString;
        }

        private string GenerateOnMsTimervents(List<MsTimerType> timerList)
        {
            string onTimerString = "";
            foreach (MsTimerType timer in timerList)
            {
                string blockContent = CaplSyntaxComponents.setMsTimerInstruction(timer);
                string sendMessagesBlock = CaplSyntaxComponents.sendMessageInstructionsBlock(timer.getAttachedMessagesList(),
                    "\tMessages to send:");
                blockContent += sendMessagesBlock;
                onTimerString += CaplSyntaxComponents.OnTimerEventBlock(blockContent,timer.MsTimerName);
            }

            return onTimerString;
        }

        private string GenerateOnKeyEvents(List<OnKeyEventHandler> keyEventsList)
        {
            string onKeyEventsString = "";
            foreach(OnKeyEventHandler keyEvent in keyEventsList)
            {
                string sendMessageBlock = CaplSyntaxComponents.sendMessageInstructionsBlock(keyEvent.messagesList,"\tMessages to send: ");
                onKeyEventsString += CaplSyntaxComponents.OnKeyPressEventBlock(sendMessageBlock, keyEvent.keySymbol);
            }
            return onKeyEventsString;
        }

        private string generateVariablesBlock()
        {
            string varBlock="";
            string varDeclarations="";

            foreach(MessageType item in globalVariables.messagesList)
            {
                    //It will be generated variables for each message
                    varDeclarations += item.getDeclartion() + CaplSyntaxConstants.NEW_LINE;
            }

            varDeclarations += CaplSyntaxConstants.NEW_LINE;

            foreach(MsTimerType item in globalVariables.msTimerList)
            {
                varDeclarations += item.getDeclartion() + CaplSyntaxConstants.NEW_LINE;
            }

            varBlock = CaplSyntaxComponents.VariablesBlock(CaplSyntaxConstants.NEW_LINE+varDeclarations);

            return varBlock;
        }

        private List<OnKeyEventHandler> getUsedKeys(List<MessageType> messages)
        {
            
            List<OnKeyEventHandler> keyEvents = new List<OnKeyEventHandler>();
            int keyEventPosition = -1;
            foreach(MessageType message in messages)
            {
                if (message.OnKey != CaplSyntaxConstants.ON_KEY_EVENT_OFF)
                {
                    for (int counter=0;counter<keyEvents.Count;counter++)
                    {
                        if(keyEvents.ElementAt(counter).keySymbol==message.OnKey)
                        {
                            keyEventPosition = counter;
                            break;
                        }
                    }

                    if (keyEventPosition >= 0)
                    {
                        keyEvents.ElementAt(keyEventPosition).messagesList.Add(message);
                        keyEventPosition = -1;
                    }
                    else
                    {
                        OnKeyEventHandler tempKeyEvent = new OnKeyEventHandler(message.OnKey);
                        tempKeyEvent.messagesList.Add(message);
                        keyEvents.Add(tempKeyEvent);
                    }
                }
            }

            return keyEvents;
        }

        public string getResult()
        {
            return fileContent;
        }
    }
}
