using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScribaService
{
    class EventRow
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
