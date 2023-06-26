using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComSimulatorApp.notifyUtilities
{
    public class NotificationMessage
    {
        //denumirea notificariii (identificatorul)
        public string Name { get; set; }
        //continutul notificariii (descriere)
        public string Content { get; set; }
        //tipul notificarii
        public NotificationTypes Type { get; set; }
        //data si ora la care a fost inregistrata notificarea
        public DateTime NotificationMoment { get; private set; }

        public string NotificationMomentString => getMomentString();

        public NotificationMessage(string Name="DEFAULT_NOTIFICATION",
            string Content="DEFAULT_CONTENT", NotificationTypes Type=NotificationTypes.Unknown)
        {
            this.Name = Name;
            this.Content = Content;
            this.Type = Type;
            this.NotificationMoment = DateTime.Now;
        }

        //converteste data si ora notificarii
        //la sir de caractere
        public string NotificationMomentToString()
        {
            string NotificationMomentString = NotificationMoment.ToString("yyyy/MM/dd HH:mm:ss");
            return NotificationMomentString;
        }

        //returneaza sirul de caractere ce reprezinta momentul
        //la care s-a inregistrat notificarea
        private string getMomentString()
        {
            return NotificationMomentToString();
        }

        //returneaza notificarea sub forma de sir de caractere
        public string MessageNotificationToString()
        {
            string notificationString = "";
            notificationString += "[" + Name + "] | ";
            notificationString += "{" + Content + "} |";
            notificationString += "{" + Type.ToString() + "} | ";
            notificationString += " @ {" + NotificationMomentToString() + "} ";

            return notificationString;

        }


    }
}
