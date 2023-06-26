using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComSimulatorApp.generalUtilities
{
    public class StringUtilities
    {
        //testeaza daca un caracter se gaseste intr-un sir
       public static bool isSpecialCh(char ch, string chSequence)
        {
           if(chSequence.Contains(ch))
            {
                return true;
            }
           else
            {
                return false;
            }
        }

        //converteste campul payload al unui mesaj din lista de octeti in
        //sir de caractere al valorilor in baza 16
        public static string  ByteListToHexaString(List<Byte> byteList)
        {
            string result="";
            if (byteList != null && byteList.Count > 0)
            {
                foreach (Byte dataByte in byteList)
                {
                    result += dataByte.ToString("X2")+" ";
                }

            }
            else
            {
                result = "0x_HEX_ERR";
            }

            return result;
        }
    }
}
