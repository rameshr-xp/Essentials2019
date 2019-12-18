

namespace AzureIotHub.Services.Runtime
{
    public interface IServicesConfig
    {
        string IoTHubConnectionString { get; set; }
        string IoTHubStorageConnectionString { get; set; }
    }

}
