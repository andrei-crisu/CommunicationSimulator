using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComSimulatorApp.fileUtilities
{
    //interfata ce defineste o serie de proprietati si 
    //metode comune pentru clasele care o implementeaza

    public interface FileTypeInterface
    {
        string fileName { get; set; }
        string fileExtension { get; set; }
        string filePath { get; set; }
        string fileContent { get; set; }
        string contentHashCode { get; set; }
        bool isModified { get; set; }
        bool isSelected { get; set; }
        bool isOpen { get; set; }

        bool isProcessed { get; set; }

        bool Open(string fileContent);
        bool Modify();
        bool Close();

        bool Save();
        
        bool Rename(string newFileName);
       bool processFile();

        string getFullName();

    }
}
