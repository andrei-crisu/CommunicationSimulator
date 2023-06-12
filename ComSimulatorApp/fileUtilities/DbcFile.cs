using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComSimulatorApp.fileUtilities
{
    public class DbcFile : FileTypeInterface
    {
        //implementare membri
        public string fileName { get; set; }
        public string fileExtension { get; set; }
        public string filePath { get; set; }
        public string fileContent { get; set; }
        public bool isOpen { get; set; }
        public bool isModified { get; set; }
        public bool isSelected { get; set; }
        public bool isProcessed { get; set; }

        //membri specifici clasei
        private dbcParserCore.DBCFileObj parsedObjects;
        public List<dbcParserCore.ParseStatusMessage> fileLog;
        public List<notifyUtilities.NotificationMessage> fileNotificationHistory;



        public DbcFile(string fileName,string content=null)
        {
            this.fileName = fileName;
            this.fileExtension = "dbc";
            this.filePath = null;
            isModified = false;
            if(content!=null)
            {
                Open(content);
            }
            else
            {
                isOpen = false;
            }

            parsedObjects = new dbcParserCore.DBCFileObj();
            fileLog = new List<dbcParserCore.ParseStatusMessage>();

            fileNotificationHistory=new List<notifyUtilities.NotificationMessage>();

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

            dbcParserCore.DBCParser dbcParser = new dbcParserCore.DBCParser();



            isProcessed = true;
            return isProcessed;
        }

        public dbcParserCore.DBCFileObj getParsededObjects()
        {
            return this.parsedObjects;
        }

        public void setParsedObjects(dbcParserCore.DBCFileObj parseObject)
        {
            this.parsedObjects = parseObject;
        }

        public string getFullName()
        {
            return this.fileName + "." + this.fileExtension;
        }

    }
}
