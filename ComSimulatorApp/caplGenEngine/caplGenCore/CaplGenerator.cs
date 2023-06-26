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
        //obiect ce stocheaza variabilele globale
        public CaplObjWorkspace globalVariables;
        //variabila fileContent va stoca continutul fisieruui generat
        //de fiecare data cand parti ale fisierului CAPL sunt generate acestea sunt adaugate
        //la continutul existent in fileContent
        private string fileContent;

        //constructorul clasei 
        public CaplGenerator(List<MessageType> messageList,List<MsTimerType> msTimerList, string initialComment="")
        {
            //setare variabile globale
            globalVariables = new CaplObjWorkspace();
            globalVariables.setMsgDataList(messageList);
            globalVariables.setMsTimerList(msTimerList);
            globalVariables.setOnKeyEvents(getUsedKeys(messageList));

            //continutul unui fisier CAPL va cuprinde intotdeauna  informatii despre codarea acestuia
            fileContent = CaplSyntaxComponents.EncodingBlock();
            //se va adauga in mod automat data si ora la care a fost generat fisierul sub forma unui comentariu
            //de asemenea se va include si comentariul generat de utilizator
            string generationMoment = DateTime.Now.ToString("yyyy/MM/dd  [HH:mm:ss]");
            fileContent += CaplSyntaxComponents.MultilineComment(initialComment + CaplSyntaxConstants.NEW_LINE+"Generated: "+generationMoment+
                CaplSyntaxConstants.NEW_LINE);
            //sunt generate event handlerele on message
            string onMsgBlock = GenerateOnMessageEvents(globalVariables.messagesList);
            //sunt generate event handlerele on timer
            string onTimerBlock = GenerateOnMsTimervents(globalVariables.msTimerList);
            //sunt generate event handlerele on key
            string onKeyBlock = GenerateOnKeyEvents(globalVariables.onKeyEvents);
            //se introduce o linie noua in fisier
            fileContent += CaplSyntaxConstants.NEW_LINE;
            //se genereaza blocul de declarare a variabilelor globale
            //si se adauga la continutul fisierului
            fileContent += generateVariablesBlock();
            //se adauga definitia functie care seteaza campul de date
            fileContent += CaplFunctionsBuilder.CaplSetPayloadFunctionDefinition();
            //se genereaza partea de setarea a valorii payload pentru fiecare mesaj in parte
            string otherContentOnStartBlock = CaplSyntaxComponents.setAllMessagesPayloadData(globalVariables.messagesList);
            //adaugare contintu pentru partea de steare a valorii payload
            fileContent += CaplSyntaxComponents.OnStartBlock(msTimerList, null, otherContentOnStartBlock, "Initialize instructions!");
            //adaugare blocuri on message
            if (onMsgBlock.Length > 0)
            {
                fileContent += (CaplSyntaxComponents.Comment("WHEN A MESSAGE IS RECEIVED EVENTS:") + CaplSyntaxConstants.NEW_LINE);
            }
            fileContent += onMsgBlock;
            //adaugare blocuri on timer
            if (onTimerBlock.Length > 0)
            {
                fileContent += (CaplSyntaxComponents.Comment("ON TIMER EVENTS:") + CaplSyntaxConstants.NEW_LINE);
            }
            fileContent += onTimerBlock;
            //adaugare blocuri on key
            if (onKeyBlock.Length > 0)
            {
                fileContent += (CaplSyntaxComponents.Comment("WHEN A KEY IS PRESSED:") + CaplSyntaxConstants.NEW_LINE);
            }
            fileContent += onKeyBlock;

        }

        //functia care genereaza blocurile on messge pentru fiecare mesaj in parte
        private string  GenerateOnMessageEvents(List<MessageType> itemsList)
        {
            string onMsgString="";
            foreach(MessageType item in itemsList)
            {
                //blocul pentru un anumit mesaj este generat doar daca 
                //optiunea OnMessage este adevarata ( a fost bifata de catre utilizator)
                // altfel se considera ca nu se doreste generarea unui eveniment de receptie
                if (item.OnMessage)
                {
                    //la receptie se afiseaza  id-ul mesajului
                    string blockContent = CaplSyntaxComponents.SimpleWriteMessageInstruction(item.varName + ".id");
                    onMsgString += CaplSyntaxComponents.OnMessageEventBlock(blockContent, item.messageName);
                }
            }

            return onMsgString;
        }

        //generarea evenimentelor on timer
        private string GenerateOnMsTimervents(List<MsTimerType> timerList)
        {
            string onTimerString = "";
            foreach (MsTimerType timer in timerList)
            {
                //in interiorul fiecarui bloc se seteaza iar timerul
                string blockContent = CaplSyntaxComponents.setMsTimerInstruction(timer);
                //se realizeaza trimiterea fiecarui mesaj asociat timer-ului
                string sendMessagesBlock = CaplSyntaxComponents.sendMessageInstructionsBlock(timer.getAttachedMessagesList(),
                    "\tMessages to send:");
                blockContent += sendMessagesBlock;
                onTimerString += CaplSyntaxComponents.OnTimerEventBlock(blockContent,timer.MsTimerName);
            }

            return onTimerString;
        }

        //generarea tuturor evenimentelor on key
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

        //functie care realizeaza declararea variabilelor globale
        private string generateVariablesBlock()
        {
            string varBlock="";
            string varDeclarations="";
            //se declara variabile pentru toate mesajele disponibile
            foreach(MessageType item in globalVariables.messagesList)
            {
                    //It will be generated variables for each message
                    varDeclarations += item.getDeclartion() + CaplSyntaxConstants.NEW_LINE;
            }

            varDeclarations += CaplSyntaxConstants.NEW_LINE;
            //se declara variabilele timer
            foreach(MsTimerType item in globalVariables.msTimerList)
            {
                varDeclarations += item.getDeclartion() + CaplSyntaxConstants.NEW_LINE;
            }

            varBlock = CaplSyntaxComponents.VariablesBlock(CaplSyntaxConstants.NEW_LINE+varDeclarations);

            return varBlock;
        }

        //fiecare mesaj stocheaza denumirea tasteie 
        //care determina trimiterea acestuia
        //exista posibilitatea insa sa se trimita mai multe mesaje la apasarea
        //aceleiasi taste.
        //prin urmare, se va crea o lista care contine tastele utilizate,
        //fiecare tasta continand mesajele carora le determina trimiterea
        private List<OnKeyEventHandler> getUsedKeys(List<MessageType> messages)
        {
            //se construieste lista ce contine tastele ( initial este goala) 
            List<OnKeyEventHandler> keyEvents = new List<OnKeyEventHandler>();
            //variabila care indica daca tasta se gaseste in lista 
            //-1 nu este un index valid, deci poate fi folosit cu incredere
            //pentru a determina daca un element se gaseste sau nu in lista
            int keyEventPosition = -1;
            //se parcurge lista de mesaje
            foreach(MessageType message in messages)
            {
                //daca tasta nu este '#' ( '#' - fiin utiliza pentru a indica ca nu se doreste 
                //trimiterea mesajului folosind apasari de taste
                if (message.OnKey != CaplSyntaxConstants.ON_KEY_EVENT_OFF)
                {
                    //se cauta daca tasta se regaseste in lista,
                    //adica daca a mai fost folosita de alt mesaj
                    for (int counter=0;counter<keyEvents.Count;counter++)
                    {
                        if(keyEvents.ElementAt(counter).keySymbol==message.OnKey)
                        {
                            keyEventPosition = counter;
                            break;
                        }
                    }

                    //daca tasta a mai fost folosita atunci keyEventPosition contine pozitia in lista
                    //si va fi deci un numar pozitiv; se va realiza adaugarea mesajului la evenimentul de tasta respectiv
                    //altfel (va fi un numar negativ) tasta nu se regaseste si se adauga in lista cu noul mesaj
                    if (keyEventPosition >= 0)
                    {
                        keyEvents.ElementAt(keyEventPosition).messagesList.Add(message);
                        keyEventPosition = -1;
                    }//altfel (va fi un numar negativ) tasta nu se regaseste si se adauga in lista cu noul mesaj
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

        //returneaza continutul fisierului generat sub forma de sir de caractere
        public string getResult()
        {
            return fileContent;
        }
    }
}
