using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComSimulatorApp.caplGenEngine.caplTypes
{
    //clasa de baza care implementeaza interfata ICaplDataType
    public class CaplDataType: ICaplDataType
    {

        public string varType {  get; set; }
        public string varName { get; set; }
        public const string DEFAULT_VAR_TYPE = "NONE_TYPE";
        public const string DEFAULT_VAR_NAME = "NONE_NAME";

    }

        
}
