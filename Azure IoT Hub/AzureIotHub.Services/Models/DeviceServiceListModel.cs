

using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace AzureIotHub.Services.Models
{
    public class DeviceServiceListModel
    {
        public string ContinuationToken { get; set; }

        public List<DeviceServiceModel> Items { get; set; }

        public DeviceServiceListModel(IEnumerable<DeviceServiceModel> devices, string continuationToken = null)
        {
            ContinuationToken = continuationToken;
            Items = new List<DeviceServiceModel>(devices);
        }

        public DeviceTwinName GetDeviceTwinNames()
        {
            if (Items?.Count > 0)
            {
                HashSet<string> tagSet = new HashSet<string>();
                HashSet<string> reportedSet = new HashSet<string>();
                Items.ForEach(m =>
                {
                    foreach (KeyValuePair<string, JToken> item in m.Twin.Tags)
                    {
                        PrepareTagNames(tagSet, item.Value, item.Key);
                    }
                    foreach (KeyValuePair<string, JToken> item in m.Twin.ReportedProperties)
                    {
                        PrepareTagNames(reportedSet, item.Value, item.Key);
                    }
                });
                return new DeviceTwinName { Tags = tagSet, ReportedProperties = reportedSet };
            }
            return null;
        }

        private void PrepareTagNames(HashSet<string> set, JToken jToken, string prefix)
        {
            if (jToken is JValue)
            {
                set.Add(prefix);
                return;
            }
            foreach (JToken item in jToken.Values())
            {
                string path = item.Path;
                PrepareTagNames(set, item, $"{prefix}.{(path.Contains(".") ? path.Substring(item.Path.LastIndexOf('.') + 1) : path)}");
            }
        }
    }
}