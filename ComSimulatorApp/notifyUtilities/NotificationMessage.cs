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
        public string Name { get; set; }
        public string Content { get; set; }
        public NotificationTypes Type { get; set; }

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

        public string NotificationMomentToString()
        {
            string NotificationMomentString = NotificationMoment.ToString("yyyy/MM/dd HH:mm:ss");
            return NotificationMomentString;
        }

        private string getMomentString()
        {
            return NotificationMomentToString();
        }

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
