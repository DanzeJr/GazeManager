using System.Collections.Generic;
using GazeManager.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GazeManager.Infrastructures
{
    public static class NotificationExtension
    {
        public static Dictionary<string, string> GetData(this Notification notification)
        {
            JsonSerializerSettings serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            serializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            serializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(notification, serializerSettings));
        }
    }
}