

using Microsoft.Azure.Devices;

namespace AzureIotHub.Services.Models
{
    public class MethodResultServiceModel
    {
        public int Status { get; set; }

        public string JsonPayload { get; set; }

        public MethodResultServiceModel()
        {
        }

        public MethodResultServiceModel(CloudToDeviceMethodResult result)
        {
            Status = result.Status;
            JsonPayload = result.GetPayloadAsJson();
        }
    }
}
