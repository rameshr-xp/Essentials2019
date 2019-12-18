

using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace AzureIotHub.Services.Extensions
{
    public static class JTokenExtension
    {
        public static IEnumerable<string> GetAllLeavesPath(this JToken root)
        {
            if (root is JValue)
            {
                yield return root.Path;
            }
            else
            {
                foreach (JToken child in root.Values())
                {
                    foreach (string name in child.GetAllLeavesPath())
                    {
                        yield return name;
                    }
                }
            }
        }
    }
}
