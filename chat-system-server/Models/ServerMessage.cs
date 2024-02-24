﻿using Newtonsoft.Json.Linq;

namespace chat_system_server.Models
{
    public enum ResponseType
    {
        OK,
        NAME_TAKEN,
        BAD_REQUEST,
        GLOBAL_MESSAGE,
        USER_JOINED,
        PRIVATE_MESSAGE,
        FIRST_TIME_POLL
    }

    public class ServerMessage
    {
        private ResponseType type;
        private string message;

        public ServerMessage(ResponseType type, string message)
        {
            this.type = type;
            this.message = message;
        }

        public ServerMessage()
        {

        }

        public void ParseFromJsonAndSet(JObject json)
        {
            type = (ResponseType)Convert.ToInt16(json["type"]);
            message = json["message"].ToString();
        }

        public string ToJsonString()
        {
            string jsonObj = $"{{\"type\": \"{(int)type}\", \"message\": \"{message}\"}}";
            return jsonObj;
        }

        public ResponseType GetResponseType()
        {
            return type;
        }

        public string GetServerMessage()
        {
            return message;
        }

        public void SetResponseType(ResponseType type)
        {
            this.type = type;
        }

        public void SetMessage(string message)
        {
            this.message = message;
        }
    }
}
