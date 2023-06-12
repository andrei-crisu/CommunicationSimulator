using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComSimulatorApp.caplGenEngine
{
    public class CaplSyntaxConstants
    {
        public const string MULTILINE_COMMENT_START = "/* ";
        public const string MULTILINE_COMMENT_END = " */";
        public const string SINGLE_LINE_COMMENT = "// ";
        public const string CODE_BLOCK_START = "\n{\n";
        public const string CODE_BLOCK_END = "\n}\n";

        public const string NEW_LINE = "\n";
        public const string TAB_STR = "\t";
        public const string END_OF_INSTRUCTION = ";";

        //to be decided what to put in it
        //for moment is empty
        public const string SPECIAL_KEYS_SEQUENCE = "";

        //define associated keyword  for the data types
        public const string KEYWORD_MESSAGE = "message";
        public const string KEYWORD_TIMER = "timer";
        public const string KEYWORD_MSTIMER = "msTimer";
        public const string KEYWORD_DOUBLE = "double";
        public const string KEYWORD_CHAR = "char";
        public const string KEYWORD_BYTE = "byte";
        public const string KEYWORD_INT = "int";
        public const string KEYWORD_long = "long";

        public const char ON_KEY_EVENT_OFF = '#';
    }
}
