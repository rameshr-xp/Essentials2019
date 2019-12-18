namespace Essentials.Business.Runtime
{
    public class ServicesConfig : AzureIotHub.Services.Runtime.IServicesConfig,IServicesConfig
    {
        public string IoTHubStorageConnectionString { get; set; }
        public string IoTHubConnectionString { get; set; }
        public string StorageConnectionString { get; set; }
    }
}
