﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chat_system_server.Models
{
    public enum ActionType
    {
        DISCONNECT,
        CONNECT,
        MESSAGE,
        PRIVATE_MESSAGE,
        RECEIVED
    }


    public class ClientMessage
    {
        public bool isValid = true;
        private ActionType actionType;
        private string userFrom;
        private string? content;
        private string? userTo;
        private DateTime timestamp;

        public ClientMessage(ActionType actionType, string userFrom, string content, string userTo)
        {
            this.actionType = actionType;
            this.userFrom = userFrom;
            this.content = content;
            this.userTo = userTo;
            timestamp = DateTime.Now;
        }

        public ClientMessage()
        {
            timestamp = DateTime.Now;
        }

        public string ToJsonString()
        {
            string jsonObj = $"{{\"actionType\": \"{actionType}\", \"userFrom\": \"{userFrom}\", \"content\": \"{content}\", \"userTo\": \"{userTo}\"}}";
            return jsonObj;
        }

        public bool ParseFromJsonAndSet(JObject json)
        {
            try
            {
                actionType = (ActionType)Convert.ToInt16(json["actionType"]);
                userFrom = json["userFrom"].ToString();
                content = json["content"].ToString();
                userTo = json["userTo"].ToString();
                isValid = true;
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public ClientMessage ParseFromJsonAndReturn(JObject json)
        {
            try
            {
                ClientMessage msg = new ClientMessage(
                (ActionType)Convert.ToInt16(json["actionType"]),
                json["userFrom"].ToString(),
                json["content"].ToString(),
                json["userTo"].ToString());

                msg.SetTimestamp(DateTime.Now);

                return msg;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                ClientMessage msg = new ClientMessage();
                msg.isValid = false;
                return msg;
            }
        }

        public void SetTimestamp(DateTime timestamp)
        {
            this.timestamp = timestamp;
        }

        public ActionType GetActionType()
        {
            return actionType;
        }

        public void SetActionType(ActionType value)
        {
            actionType = value;
        }

        public string GetUserFrom()
        {
            return userFrom;
        }

        public void SetUserFrom(string value)
        {
            userFrom = value;
        }

        public string? GetContent()
        {
            return content;
        }

        public void SetContent(string? value)
        {
            content = value;
        }

        public string? GetUserTo()
        {
            return userTo;
        }

        public void SetUserTo(string? value)
        {
            userTo = value;
        }

    }
}
