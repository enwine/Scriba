using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ScribaService
{
    internal class EventRow
    {
        public string ApplicationId { get; set; }
        public string EventId { get; set; }
        public string EventDescription { get; set; }
        public Dictionary<string, string> Fields { get; set; }

        public static EventRow FromJson(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<EventRow>(json);
            }
            catch (Exception ex)
            {
                throw new Exception("Couldn't deserialize EventRow from Json string.", ex);
            }
        }
    }
}