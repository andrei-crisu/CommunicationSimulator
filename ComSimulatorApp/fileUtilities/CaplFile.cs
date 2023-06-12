using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComSimulatorApp.caplGenEngine;

namespace ComSimulatorApp.fileUtilities
{
    public class CaplFile : FileTypeInterface
    {

        public string fileName { get; set; }
        public string fileExtension { get; set; }
        public string filePath { get; set; }
        public string fileContent { get; set; }
        public bool isOpen { get; set; }
        public bool isModified { get; set; }
        public bool isSelected { get; set; }
        public bool isProcessed { get; set; }

        public CaplObjWorkspace globalVariables;


        public CaplFile(string fileName,string content)
        {
            this.fileName = fileName;
            this.fileExtension = "can";
            this.filePath = null;
            isModified = false;
            if (content != null)
            {
                Open(content);
            }
            else
            {
                isOpen = false;
            }
            isSelected = false;
            isProcessed = false;

            globalVariables = new CaplObjWorkspace();

        }
        public bool Open(string fileContent)
        {
            isOpen = true;
            this.fileContent = fileContent;
            return isOpen;

        }

        public bool Modify()
        {
            this.isModified = true;
            return isModified;
        }

        public bool Close()
        {
            bool isClosed = false;
            isClosed = true;
            return isClosed;
        }

        public bool Save()
        {
            isModified = false;
            return (!isModified);
        }

        public bool Rename(string newName)
        {
            fileName = newName;
            return true;
        }


        public bool processFile()
        {
            isProcessed = false;


            isProcessed = true;
            return isProcessed;
        }

        public string getFullName()
        {
            return this.fileName + "." + this.fileExtension;
        }
    }
}
