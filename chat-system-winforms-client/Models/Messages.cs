using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chat_system_winforms.Models
{
    public enum ActionType
    {
        Disconnect,
        Connect,
        Message,
        PrivateMessage
    }

    public class Message
    {
        private ActionType actionType;
        private string userFrom;
        private string? content;
        private string? userTo;

        public Message(ActionType actionType, string userFrom, string content, string userTo)
        {
            this.actionType = actionType;
            this.userFrom = userFrom;
            this.content = content;
            this.userTo = userTo;
        }



    }
}
