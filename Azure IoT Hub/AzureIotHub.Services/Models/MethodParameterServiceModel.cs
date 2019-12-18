

using Microsoft.Azure.Devices;
using System;

namespace AzureIotHub.Services.Models
{
    public class MethodParameterServiceModel
    {
        public string Name { get; set; }

        public TimeSpan? ResponseTimeout { get; set; }

        public TimeSpan? ConnectionTimeout { get; set; }

        public string JsonPayload { get; set; }

        public MethodParameterServiceModel()
        {
        }

        public MethodParameterServiceModel(CloudToDeviceMethod azureModel)
        {
            Name = azureModel.MethodName;
            ResponseTimeout = azureModel.ResponseTimeout;
            ConnectionTimeout = azureModel.ConnectionTimeout;
            JsonPayload = azureModel.GetPayloadAsJson();
        }

        public CloudToDeviceMethod ToAzureModel()
        {
            CloudToDeviceMethod method = new CloudToDeviceMethod(Name);
            if (ResponseTimeout.HasValue)
            {
                method.ResponseTimeout = ResponseTimeout.Value;
            }

            if (ConnectionTimeout.HasValue)
            {
                method.ConnectionTimeout = ConnectionTimeout.Value;
            }

            if (!string.IsNullOrEmpty(JsonPayload))
            {
                method.SetPayloadJson(JsonPayload);
            }

            return method;
        }
    }
}
