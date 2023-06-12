using ComSimulatorApp.notifyUtilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ComSimulatorApp.dbcParserCore
{
    public class DBCParser
    {
        private DBCFileObj parsedFile;
        private Boolean parseStatus;
        //stocheaza toate mesajele de eroarea, avertisementele,mesajele informative
        //generate de parser
        public List<ParseStatusMessage> parserLog;
        private List<NotificationMessage> parserNotificationHistory;

        public DBCParser()
        {
            parsedFile = new DBCFileObj();
            parseStatus = false;
            parserLog = new List<ParseStatusMessage>();
            parserNotificationHistory = new List<NotificationMessage>();


        }


        private Boolean parseNodes(string nodeListString)
        {
            Boolean parseStatus = false;



            return parseStatus;
        }

        //extrage informatia din al patrulea token din descrierea mesajului
        //acesta corespunde tokenului ce contine informatii despre:
        // bitul de start, lungimea in biti, ordinea byte-ilor in cuvant ( Intel/Motorola),
        //precum si semnul
        public Boolean getForthTokenInfo(string token, out uint startBit, out uint length, out uint byteOrder, out char sign)
        {
            Boolean status = false;
            startBit = 0;
            length = 0;
            byteOrder = ParserConstants.BYTE_ORDER_ERR;
            sign = '?';

            token = token.Trim();
            string[] subtokens = token.Split("@");
            if (subtokens.Length != 2)
            {
                parserLog.Add(new ParseStatusMessage("Found a signal with token { " + token + 
                    " } that can't be parsed!It contains incorrect information :: [ '@' ] separator",
                    ParserConstants.ParserMsgTypes.ERROR));

                RegisterNotificationMessage(new NotificationMessage(NotificationNames.ERR_PARSE,
                    "Found a signal with token { " + token +
                    " } that can't be parsed!It contains incorrect information :: around the [ '@' ] separator",
                    NotificationTypes.Error));
                return parseStatus = false;
            }
            else
            {
                //acest token contine informatia
                //despre bitul de start al semnalului in cadrul mesajului si numarul de biti ca lungime
                subtokens[0] = subtokens[0].Trim();
                string[] startBitAndLenghtTokens = subtokens[0].Split("|");
                //daca nu este format din doua tokens separate print '|'
                //atunci este eronat
                if (startBitAndLenghtTokens.Length != 2)
                {
                    parserLog.Add(new ParseStatusMessage("Found a signal with token { " + token + 
                        " } that can't be parsed!It contains incorrect information :: [ '|' ] separator",
                  ParserConstants.ParserMsgTypes.ERROR));

                    RegisterNotificationMessage(new NotificationMessage(NotificationNames.ERR_PARSE,
                    "Found a signal with token { " + token +
                        " } that can't be parsed!It contains incorrect information :: [ '|' ] separator",
                    NotificationTypes.Error));
                    return parseStatus = false;
                }
                else
                {
                    bool succes = false;
                    succes = uint.TryParse(startBitAndLenghtTokens[0], out uint parsedDataStartBit);
                    if (succes)
                    {
                        startBit = parsedDataStartBit;
                    }
                    else
                    {
                        parserLog.Add(new ParseStatusMessage("Found a signal with token { " + token + 
                            " } that can't be parsed!Can't parse <startBit field>",
                            ParserConstants.ParserMsgTypes.ERROR));

                        RegisterNotificationMessage(new NotificationMessage(NotificationNames.ERR_PARSE,
                   "Found a signal with token { " + token +
                            " } that can't be parsed!Can't parse <startBit field>",
                   NotificationTypes.Error));
                        return parseStatus = false;
                    }

                    succes = false;
                    succes = uint.TryParse(startBitAndLenghtTokens[1], out uint parseDataLength);
                    if (succes)
                    {
                        length = parseDataLength;
                    }
                    else
                    {
                        parserLog.Add(new ParseStatusMessage("Found a signal with token { " + token + 
                            " } that can't be parsed!Can't parse <length field>",
                            ParserConstants.ParserMsgTypes.ERROR));


                        RegisterNotificationMessage(new NotificationMessage(NotificationNames.ERR_PARSE,
                   "Found a signal with token { " + token +
                            " } that can't be parsed!Can't parse <length field>",
                   NotificationTypes.Error));
                        return parseStatus = false;
                    }
                }

                //incercare extragere endianess si semn
                string byteOrderAndSign = subtokens[1].Trim();
                if (byteOrderAndSign.Length != 2)
                {
                    parserLog.Add(new ParseStatusMessage("Found a signal with token { " + token +
                        " } that can't be parsed!Can't parse <{Endianness,Sign}> fields>",
                    ParserConstants.ParserMsgTypes.ERROR));

                    RegisterNotificationMessage(new NotificationMessage(NotificationNames.ERR_PARSE,
                  "Found a signal with token { " + token +
                        " } that can't be parsed!Can't parse <{Endianness,Sign}> fields>",
                  NotificationTypes.Error));
                    return parseStatus = false;
                }
                else
                {
                    uint byteOrderVal;
                    string strByteOrder = byteOrderAndSign[0].ToString();
                    if (uint.TryParse(strByteOrder,out uint outByteOrderVal))
                    {
                        byteOrderVal = outByteOrderVal;
                    }
                    else
                    {
                        parserLog.Add(new ParseStatusMessage("Found a signal with token { " + strByteOrder +
                               " } that can't be parsed!Can't extract ENDIANNESS number!",
                                ParserConstants.ParserMsgTypes.ERROR));

                        RegisterNotificationMessage(new NotificationMessage(NotificationNames.ERR_PARSE,
                  "Found a signal with token { " + strByteOrder +
                               " } that can't be parsed!Can't extract ENDIANNESS number!",
                  NotificationTypes.Error));
                        return parseStatus = false;
                    }

                    char signValue = (char)byteOrderAndSign[1];

                    switch (byteOrderVal)
                    {
                        case (uint)ENDIANNESS.INTEL:
                            byteOrder = (uint)ENDIANNESS.INTEL;
                            break;
                        case (uint)ENDIANNESS.MOTOROLA:
                            byteOrder = (uint)ENDIANNESS.MOTOROLA;
                            break;

                        default:
                            parserLog.Add(new ParseStatusMessage("Found a signal with token { " + token +
                                " } that can't be parsed!Unknown <Endianness format> :: "+byteOrderVal,
                                 ParserConstants.ParserMsgTypes.ERROR));

                            RegisterNotificationMessage(new NotificationMessage(NotificationNames.ERR_PARSE,
                "Found a signal with token { " + token +
                                " } that can't be parsed!Unknown <Endianness format> :: " + byteOrderVal,NotificationTypes.Error));
                            return parseStatus = false;
                    }

                    switch (signValue)
                    {
                        case (char)SIGN_VAL.SIGNED_VALUE:
                            sign = (char)SIGN_VAL.SIGNED_VALUE;
                            break;

                        case (char)SIGN_VAL.UNSIGNED_VALUE:
                            sign = (char)SIGN_VAL.UNSIGNED_VALUE;
                            break;

                        default:
                            parserLog.Add(new ParseStatusMessage("Found a signal with token { " + token +
                                " } that can't be parsed!Unknown <SIGN format> :: " + signValue,
                                ParserConstants.ParserMsgTypes.ERROR));

                            RegisterNotificationMessage(new NotificationMessage(NotificationNames.ERR_PARSE,
                                    "Found a signal with token { " + token +
                                    " } that can't be parsed!Unknown <SIGN format> :: " + signValue,NotificationTypes.Error));
                            return parseStatus = false;

                    }

                }
            }


            status = true;
            return status;
        }

        public Boolean getScaleAndOffset(string token, out float scale, out float offset)
        {
            Boolean status = false;
            scale = 0;
            offset = 0;
            token = token.Replace("(","");
            token = token.Replace(")", "");
            token = token.Trim();
            string[] subtokenList = token.Split(",");
            if (subtokenList.Length != 2)
            {
                parserLog.Add(new ParseStatusMessage("Found a signal with token { " + token + " } that can't be parsed!It contains incorrect information",
                            ParserConstants.ParserMsgTypes.ERROR));

                RegisterNotificationMessage(new NotificationMessage(NotificationNames.ERR_PARSE,
                                    "Found a signal with token { " + token + " } that can't be parsed!It contains incorrect information",
                             NotificationTypes.Error));
                return status = false;
            }
            else
            {
                Boolean dataParseSuccess = false;
                dataParseSuccess = float.TryParse(subtokenList[0], out float outScale);
                if (dataParseSuccess)
                {
                    scale = outScale;
                }
                else
                {
                    parserLog.Add(new ParseStatusMessage("Found a signal with token { " + token + " } that can't be parsed!It contains incorrect information",
                            ParserConstants.ParserMsgTypes.ERROR));


                    RegisterNotificationMessage(new NotificationMessage(NotificationNames.ERR_PARSE,
                                        "Found a signal with token { " + token + " } that can't be parsed!It contains incorrect information",
                                 NotificationTypes.Error));
                    return status = false;
                }


                dataParseSuccess = false;
                dataParseSuccess = float.TryParse(subtokenList[1], out float outOffset);
                if (dataParseSuccess)
                {
                    offset = outOffset;
                }
                else
                {
                    parserLog.Add(new ParseStatusMessage("Found a signal with token { " + token + " } that can't be parsed!It contains incorrect information",
                            ParserConstants.ParserMsgTypes.ERROR));

                    RegisterNotificationMessage(new NotificationMessage(NotificationNames.ERR_PARSE,
                                        "Found a signal with token { " + token + " } that can't be parsed!It contains incorrect information",
                                 NotificationTypes.Error));

                    return status = false;
                }
            }
            status = true;
            return status;
        }

        public Boolean getMinMax(string token, out float minValue, out float maxValue)
        {
            Boolean status=false;
            minValue = 121;
            maxValue = -121;
            token = token.Replace("[", "");
            token = token.Replace("]", "");
            token = token.Trim();
            string[] subtokenList = token.Split("|");
            if (subtokenList.Length != 2)
            {
                parserLog.Add(new ParseStatusMessage("Found a signal with token { " + token + " } that can't be parsed!It contains incorrect information",
                            ParserConstants.ParserMsgTypes.ERROR));

                RegisterNotificationMessage(new NotificationMessage(NotificationNames.ERR_PARSE,
                                        "Found a signal with token { " + token + " } that can't be parsed!It contains incorrect information",
                                 NotificationTypes.Error));
                return status = false;
            }
            else
            {
                Boolean dataParseSuccess = false;
                dataParseSuccess = float.TryParse(subtokenList[0], out float outMin);
                if (dataParseSuccess)
                {
                    minValue = outMin;
                }
                else
                {
                    parserLog.Add(new ParseStatusMessage("Found a signal with token { " + token + " } that can't be parsed!It contains incorrect information",
                            ParserConstants.ParserMsgTypes.ERROR));

                    RegisterNotificationMessage(new NotificationMessage(NotificationNames.ERR_PARSE,
                                        "Found a signal with token { " + token + " } that can't be parsed!It contains incorrect information",
                                 NotificationTypes.Error));
                    return status = false;
                }


                dataParseSuccess = false;
                dataParseSuccess = float.TryParse(subtokenList[1], out float outMax);
                if (dataParseSuccess)
                {
                    maxValue = outMax;
                }
                else
                {
                    parserLog.Add(new ParseStatusMessage("Found a signal with token { " + token + " } that can't be parsed!It contains incorrect information",
                            ParserConstants.ParserMsgTypes.ERROR));

                    RegisterNotificationMessage(new NotificationMessage(NotificationNames.ERR_PARSE,
                                        "Found a signal with token { " + token + " } that can't be parsed!It contains incorrect information",
                                 NotificationTypes.Error));
                    return status = false;
                }
            }
            status = true;
            return status;
        }

        public Boolean getSignalUnit(string token, out string outSignalUnit)
        {
            Boolean status = false;
            outSignalUnit = "";
            token = token.Trim();
            token = token.Replace("\"", "");
            if(token.Length==0)
            {
                outSignalUnit = "";
            }
            else
            {
                outSignalUnit = token;
            }
            status = true;
            return status;
        }

        public Boolean parseSignalInformation(string[] signalLineTokens,out Signal signal)
        {
            Boolean parseStatus = false;
            signal = new Signal();

            if(signalLineTokens.Length!=ParserConstants.SIGNAL_TOKEN_NUMBER)
            {
                string tokenListAsString = "SIGNAL tokens ERROR| number of tokens is invalid| ";
                tokenListAsString = tokenListAsString + "expected: " + ParserConstants.SIGNAL_TOKEN_NUMBER.ToString();
                tokenListAsString = tokenListAsString + ", provided: " + signalLineTokens.Length.ToString() + "| Token List: ";
                foreach (string token in signalLineTokens)
                {
                    tokenListAsString += "{";
                    tokenListAsString += token;
                    tokenListAsString += "} ";
                }
                parserLog.Add(new ParseStatusMessage(tokenListAsString, 
                    ParserConstants.ParserMsgTypes.ERROR));

                RegisterNotificationMessage(new NotificationMessage(NotificationNames.ERR_PARSE,
                                        tokenListAsString, NotificationTypes.Error));
                return parseStatus = false;
            }
            else
            {
                signal.setSingnalName(signalLineTokens[1]);

                //get startBit,length,byteOrder,and sign for signal
                Boolean tokenParseStatus = false;
                tokenParseStatus=getForthTokenInfo(signalLineTokens[3],out uint startBit, out uint length ,out uint byteOrder, out char sign);
                if(tokenParseStatus)
                {
                    signal.setStartBit(startBit);
                    signal.setLength(length);
                    signal.setByteOrder(byteOrder);
                    signal.setSign(sign);
                }
                else
                {
                    parserLog.Add(new ParseStatusMessage("Found a signal with token { " + signalLineTokens[3] + 
                        " } that can't be parsed!Some tokens can't be extracted!",
                    ParserConstants.ParserMsgTypes.ERROR));

                    RegisterNotificationMessage(new NotificationMessage(NotificationNames.ERR_PARSE,
                                       "Found a signal with token { " + signalLineTokens[3] +
                        " } that can't be parsed!Some tokens can't be extracted!", NotificationTypes.Error));
                    return parseStatus = false;
                }

                //parse the scale and offset
                tokenParseStatus = false;
                tokenParseStatus = getScaleAndOffset(signalLineTokens[4], out float scaleFactor, out float offsetValue);
                if (tokenParseStatus)
                {
                    signal.setFactor(scaleFactor);
                    signal.setOffset(offsetValue);
                }
                else
                {
                    parserLog.Add(new ParseStatusMessage("Found a signal with token { " + signalLineTokens[4] + " } that can't be parsed!It contains incorrect information",
                   ParserConstants.ParserMsgTypes.ERROR));

                    RegisterNotificationMessage(new NotificationMessage(NotificationNames.ERR_PARSE,
                                      "Found a signal with token { " + signalLineTokens[4] + 
                                      " } that can't be parsed!It contains incorrect information",NotificationTypes.Error));
                    return parseStatus = false;
                }

                //parse the min and max values
                tokenParseStatus = false;
                tokenParseStatus= getMinMax(signalLineTokens[5], out float minValue, out float maxValue);
                if (tokenParseStatus)
                {
                    signal.setMinValue(minValue);
                    signal.setMaxValue(maxValue);
                }
                else
                {
                    parserLog.Add(new ParseStatusMessage("Found a signal with token { " + signalLineTokens[5] + " } that can't be parsed!It contains incorrect information",
                  ParserConstants.ParserMsgTypes.ERROR));

                    RegisterNotificationMessage(new NotificationMessage(NotificationNames.ERR_PARSE,
                                    "Found a signal with token { " + signalLineTokens[5] + " } that can't be parsed!It contains incorrect information"
                  , NotificationTypes.Error));
                    return parseStatus = false;
                }

                //parse the unit
                tokenParseStatus = false;
                tokenParseStatus = getSignalUnit(signalLineTokens[6],out string outSignalUnit);
                if (tokenParseStatus)
                {
                    signal.setSignalUnit(outSignalUnit);
                }
                else
                {
                    parserLog.Add(new ParseStatusMessage("Found a signal with token { " + signalLineTokens[6] + " } that can't be parsed!It contains incorrect information",
                  ParserConstants.ParserMsgTypes.ERROR));

                    RegisterNotificationMessage(new NotificationMessage(NotificationNames.ERR_PARSE,
                                    "Found a signal with token { " + signalLineTokens[6] + " } that can't be parsed!It contains incorrect information",
                  NotificationTypes.Error));
                    return parseStatus = false;
                }




                //parse the receiver node
                //the code was addapted to parse correctly only one receiver
                //there is no possiblity to pars correct the message for more than one receivers
                List<Node> receivingNodes = new List<Node>();
                receivingNodes.Add(new Node(signalLineTokens[7].Trim()));
                signal.setReceivingNodes(receivingNodes);


            }
            parseStatus = true;
            return parseStatus;
        }

        public Boolean parseMessageInformation(string[] messageLineTokens,out Message msg)
        {
            msg = new Message();
            Boolean parseStatus = false;
            if(messageLineTokens.Length!=ParserConstants.MESSAGE_TOKEN_NUMBER)
            {
                string tokenListAsString="MESSAGE tokens ERROR| number of tokens is invalid| ";
                tokenListAsString = tokenListAsString + "expected: " + ParserConstants.MESSAGE_TOKEN_NUMBER.ToString();
                tokenListAsString = tokenListAsString + ", provided: " + messageLineTokens.Length.ToString() + "| Token List: ";
                foreach (string token in messageLineTokens)
                {
                    tokenListAsString += "{";
                    tokenListAsString += token;
                    tokenListAsString += "} ";
                }
                parserLog.Add(new ParseStatusMessage(tokenListAsString, 
                    ParserConstants.ParserMsgTypes.ERROR));

                RegisterNotificationMessage(new NotificationMessage(NotificationNames.ERR_PARSE,
                                   tokenListAsString, NotificationTypes.Error));
            }
            else
            {
                bool succes = false;
                succes = uint.TryParse(messageLineTokens[1], out uint canid);

                if (succes)
                {
                    msg.setCanId(canid);
                }
                else
                {
                    parserLog.Add(new ParseStatusMessage(messageLineTokens[1]+ "can't be interpreted as can ID for a message", 
                        ParserConstants.ParserMsgTypes.ERROR));

                    RegisterNotificationMessage(new NotificationMessage(NotificationNames.ERR_PARSE,
                                  messageLineTokens[1] + "can't be interpreted as can ID for a message", NotificationTypes.Error));
                    return parseStatus = false;
                }

                string messageName = messageLineTokens[2].Replace(":", "");
                msg.setMessageName(messageName);

                succes = uint.TryParse(messageLineTokens[3], out uint length);

                if (succes)
                {
                    msg.setMessageLength(length);
                }
                else
                {
                    parserLog.Add(new ParseStatusMessage(messageLineTokens[3] + "can't be interpreted as  length (in bytes) for a message", 
                        ParserConstants.ParserMsgTypes.ERROR));

                    RegisterNotificationMessage(new NotificationMessage(NotificationNames.ERR_PARSE,
                                  messageLineTokens[3] + "can't be interpreted as  length (in bytes) for a message"
                        , NotificationTypes.Error));
                    return parseStatus = false;
                }

                msg.setSendingNode(new Node(messageLineTokens[4]));
                parseStatus = true;

            }

            return parseStatus;
        }

        public Boolean parseFile(string fileContent)
        {
            parseStatus = false;

            string delimiter = "\n";
            string[] dbcFileLines = fileContent.Split(delimiter);
            for (int i = 0; i < dbcFileLines.Length; i++)
            {
                string line = dbcFileLines[i].Trim();
                //only for debugging to see that the lines are read in the right way
                //RegisterNotificationMessage(new NotificationMessage(NotificationNames.INFO_0002, line, NotificationTypes.Information));
                if (line.Length > 3)
                {
                    //replace 2 and 3 spaces with one
                    //this is necessary when splitting the string
                    //the delimiter is considered a simple space
                    //otherwise can be generated parsing errors
                    line.Replace("   ", " ");
                    line.Replace("  ", " ");
                    parseStatus=parseLine(line);

                    if(parseStatus==false)
                    {
                        this.parserLog.Add(new ParseStatusMessage("PARSE FILE ERRR: Line: " + line + " can't be parsed! WRONG SYNTAX!",
                               ParserConstants.ParserMsgTypes.ERROR));

                        RegisterNotificationMessage(new NotificationMessage(NotificationNames.ERR_PARSE,
                            "PARSE FILE ERRR: Line: " + line + " can't be parsed! WRONG SYNTAX!", NotificationTypes.Error));
                        return parseStatus;
                    }
                    else
                    {
                        //the file was parsed correctly
                        //the parser will continue the parsing process
                        parseStatus = true;
                    }
                }
            }


            return parseStatus;
        }

        public Boolean parseLine(string dbcFileLine)
        {
            Boolean parseStatus=false;
            string line = dbcFileLine.Trim();


            string[] dbcLineTokens = line.Split(' ');
            int tokenListLen = dbcLineTokens.Length;
            if(tokenListLen <= 1)
            {
                return parseStatus = true;
            }
            else
            {
                List<string> validTokens = new List<string>();
                foreach (string tokenTovalidate in dbcLineTokens)
                {
                    if (!string.IsNullOrEmpty(tokenTovalidate))
                    {
                        validTokens.Add(tokenTovalidate);
                    }
                }

                string[] lineTokens = validTokens.ToArray();

                //pe baza primului token din lista se determina ce 
                //fel de informatii cuprinde acea lista parsata
                //ex: poate fi o lista de noduri, descrierea unui mesaj
                //sau descrierea unui semnal
                switch (lineTokens[0])
                {
                    
                    case dbcFileFormatConstants.CAN_NODES_LIST_TAG:
                        //! THIS CAN BE REPLACED WITH A FUNCTION
                        for(int i=1;i< tokenListLen; i++)
                        {
                            this.parsedFile.nodes.Add(new Node(lineTokens[i]));
                        }
                        parseStatus = true;
                        break;

                    case dbcFileFormatConstants.MESSAGE_TAG:
                        Boolean parseSucces=parseMessageInformation(lineTokens, out Message parsedMessage);
                        if(parseSucces)
                        {
                            this.parsedFile.messages.Add(parsedMessage);
                            parseStatus = true;
                        }
                        else
                        {
                            this.parsedFile.messages.Add(new Message());
                            this.parserLog.Add(new ParseStatusMessage("Line: " + line + " can't be parsed as a message!",
                                ParserConstants.ParserMsgTypes.ERROR));

                            RegisterNotificationMessage(new NotificationMessage(NotificationNames.ERR_PARSE,
                            "Line: " + line + " can't be parsed as a message!", NotificationTypes.Error));
                            parseStatus = false;
                        }

                        break;

                    case dbcFileFormatConstants.SIGNAL_TAG:
                        if(this.parsedFile.messages.Count()==0)
                        {
                            this.parsedFile.messages.Add(new Message());
                        }
                        parseSucces=parseSignalInformation(lineTokens,out Signal parsedSignal);
                        int lastIndex = this.parsedFile.messages.Count() - 1;

                        if (parseSucces)
                        {
                            this.parsedFile.messages[lastIndex].signals.Add(parsedSignal);
                            parseStatus = true;
                        }
                        else
                        {
                            this.parsedFile.messages[lastIndex].signals.Add(new Signal());
                            this.parserLog.Add(new ParseStatusMessage("Line: " + line + " can't be parsed as signal!",
                                ParserConstants.ParserMsgTypes.ERROR));

                            RegisterNotificationMessage(new NotificationMessage(NotificationNames.ERR_PARSE,
                           "Line: " + line + " can't be parsed as signal!", NotificationTypes.Error));
                            parseStatus = false;

                        }

                        break;

                    default:
                        //in case there is another tag , other than the specified ones
                        //nothing happens
                        //the files is not parsed
                        parseStatus = true;
                        break;
                }


                
            }



            return parseStatus;
        }


        public DBCFileObj getParsedResult()
        {
            return this.parsedFile;
        }

        public Boolean getParseStatus()
        {
            return this.parseStatus;
        }


        public void RegisterNotificationMessage(NotificationMessage notificationMessage)
        {
            parserNotificationHistory.Add(notificationMessage);
        }

        public List<NotificationMessage> getParserNotificationMessages()
        {
            return parserNotificationHistory;
        }
    }

}
