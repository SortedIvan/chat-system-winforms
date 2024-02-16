using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace chat_system_server.Models
{
    public enum ResponseType
    {
        OK,
        NAME_TAKEN,
        BAD_REQUEST
    }

    public class ServerResponse
    {
        private ResponseType type;
        private string message;

        public ServerResponse(ResponseType type, string message)
        {
            this.type = type;
            this.message = message;
        }

        public ServerResponse()
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
