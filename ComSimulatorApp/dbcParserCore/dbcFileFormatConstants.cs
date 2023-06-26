using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComSimulatorApp.dbcParserCore
{
    public class dbcFileFormatConstants
    {

        //NS_ tag
        public const string NS_TAG = "NS_";

        //tag pentru configurarea magistralei
        public const string CONFIRURATION_TAG = "BS_";

        //tag care indica lista de noduri
        public const string CAN_NODES_LIST_TAG = "BU_:";

        //tag care indica sintaxa unui mesaj
        public const string MESSAGE_TAG = "BO_";

        //tag care indica sintaxa unui semnal
        public const string SIGNAL_TAG = "SG_";

        //tag de descriere
        public const string DESCRIPTION_TAG = "CM_";

        //tag pentru definierea unui atribut
        public const string ATTRIBUTE_DEFINITION_TAG = "BA_DEF_";

        //valoarea default a unui atribut
        public const string ATTRIBUTE_DEFAULT_VAL_TAG = "BA_DEF_DEF_";

        //semanalele care nu au setat un destinatar vor fi setate la 
        //destinatar cu aceasta denumire
        public const string NO_RECEIVER= "Vector__XXX";

    }

    public enum ENDIANNESS
    {
        //big-endian
        MOTOROLA = 0,
        //little-endian
        INTEL = 1,
        
        //unknonwn ( error flag for endiannness)
        UNKNOWN=99
    }

    public enum SIGN_VAL
    {
        //cu semn
        SIGNED_VALUE = '-',
        //fara semn
        UNSIGNED_VALUE = '+'
    }
}
