

using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace AzureIotHub.Services.Models
{
    public class DeviceTwinServiceModel
    {
        public string ETag { get; set; }
        public string DeviceId { get; set; }
        public bool IsSimulated { get; set; }
        public Dictionary<string, JToken> DesiredProperties { get; set; }
        public Dictionary<string, JToken> ReportedProperties { get; set; }
        public Dictionary<string, JToken> Tags { get; set; }
        public Microsoft.Azure.Devices.DeviceConnectionState? DeviceConnectionState { get; set; }
        public DateTime? LastActivityTime { get; set; }

        public DeviceTwinServiceModel()
        {
        }

        public DeviceTwinServiceModel(
            string etag,
            string deviceId,
            Dictionary<string, JToken> desiredProperties,
            Dictionary<string, JToken> reportedProperties,
            Dictionary<string, JToken> tags,
            bool isSimulated)
        {
            ETag = etag;
            DeviceId = deviceId;
            DesiredProperties = desiredProperties;
            ReportedProperties = reportedProperties;
            Tags = tags;
            IsSimulated = isSimulated;

        }

        public DeviceTwinServiceModel(Twin twin)
        {
            if (twin != null)
            {
                ETag = twin.ETag;
                DeviceId = twin.DeviceId;
                Tags = TwinCollectionToDictionary(twin.Tags);
                DesiredProperties = TwinCollectionToDictionary(twin.Properties.Desired);
                ReportedProperties = TwinCollectionToDictionary(twin.Properties.Reported);
                IsSimulated = Tags.ContainsKey("IsSimulated") && Tags["IsSimulated"].ToString() == "Y";
                DeviceConnectionState = twin.ConnectionState ?? Microsoft.Azure.Devices.DeviceConnectionState.Disconnected;
                LastActivityTime = twin.LastActivityTime;
            }
        }

        public Twin ToAzureModel()
        {
            TwinProperties properties = new TwinProperties
            {
                Desired = DictionaryToTwinCollection(DesiredProperties),
                Reported = DictionaryToTwinCollection(ReportedProperties),
            };

            return new Twin(DeviceId)
            {
                ETag = ETag,
                Tags = DictionaryToTwinCollection(Tags),
                Properties = properties
            };
        }

        /*
        JValue:  string, integer, float, boolean
        JArray:  list, array
        JObject: dictionary, object

        JValue:     JToken, IEquatable<JValue>, IFormattable, IComparable, IComparable<JValue>, IConvertible
        JArray:     JContainer, IList<JToken>, ICollection<JToken>, IEnumerable<JToken>, IEnumerable
        JObject:    JContainer, IDictionary<string, JToken>, ICollection<KeyValuePair<string, JToken>>, IEnumerable<KeyValuePair<string, JToken>>, IEnumerable, INotifyPropertyChanged, ICustomTypeDescriptor, INotifyPropertyChanging
        JContainer: JToken, IList<JToken>, ICollection<JToken>, IEnumerable<JToken>, IEnumerable, ITypedList, IBindingList, IList, ICollection, INotifyCollectionChanged
        JToken:     IJEnumerable<JToken>, IEnumerable<JToken>, IEnumerable, IJsonLineInfo, ICloneable, IDynamicMetaObjectProvider
        */
        private static Dictionary<string, JToken> TwinCollectionToDictionary(TwinCollection x)
        {
            Dictionary<string, JToken> result = new Dictionary<string, JToken>();

            if (x == null)
            {
                return result;
            }

            foreach (KeyValuePair<string, object> twin in x)
            {
                try
                {
                    if (twin.Value is JToken)
                    {
                        result.Add(twin.Key, (JToken)twin.Value);
                    }
                    else
                    {
                        result.Add(twin.Key, JToken.Parse(twin.Value.ToString()));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }

            return result;
        }

        private static TwinCollection DictionaryToTwinCollection(Dictionary<string, JToken> x)
        {
            TwinCollection result = new TwinCollection();

            if (x != null)
            {
                foreach (KeyValuePair<string, JToken> item in x)
                {
                    try
                    {
                        result[item.Key] = item.Value;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }

            return result;
        }
    }
}
