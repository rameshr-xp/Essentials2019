using AzureIotHub.Services.Exceptions;
using AzureIotHub.Services.Extensions;
using AzureIotHub.Services.Helpers;
using AzureIotHub.Services.Models;
using AzureIotHub.Services.Runtime;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureIotHub.Services
{

    public delegate Task<DevicePropertyServiceModel> DevicePropertyDelegate(DevicePropertyServiceModel model);
    public interface IDeviceRegistryService
    {
        Task<DeviceServiceListModel> GetListAsync(string query, string continuationToken);
        Task<DeviceTwinName> GetDeviceTwinNamesAsync();
        Task<DeviceTwinServiceModel> GetDeviceTwinAsync(string id);
        Task<DeviceServiceModel> GetAsync(string id);
        Task<DeviceServiceModel> CreateAsync(DeviceServiceModel toServiceModel);
        Task<DeviceServiceModel> CreateWithDesiredAsync(DeviceServiceModel device, string twinPatch);
        Task<DeviceServiceModel> CreateOrUpdateAsync(DeviceServiceModel toServiceModel, DevicePropertyDelegate devicePropertyDelegate);
        Task DeleteAsync(string id);
        Task<MethodResultServiceModel> InvokeDeviceMethodAsync(string deviceId, MethodParameterServiceModel parameter);
        Task<bool> UpdateDeviceTwinAsync(DeviceServiceModel device);
        Task<bool> UpdateDeviceTwinAsync(string deviceId, string twinPatch);
    }

    public class DeviceRegistryService : IDeviceRegistryService
    {
        private const int MAX_GET_LIST = 1000;
        private const string QUERY_PREFIX = "SELECT * FROM devices";

        private RegistryManager registry;
        private string ioTHubHostName;
        private ServiceClient serviceClient;

        public DeviceRegistryService(
            IServicesConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            IoTHubConnectionHelper.CreateUsingHubConnectionString(config.IoTHubConnectionString, (conn) =>
            {
                registry = RegistryManager.CreateFromConnectionString(conn);
                ioTHubHostName = IotHubConnectionStringBuilder.Create(conn).HostName;
                serviceClient = ServiceClient.CreateFromConnectionString(conn);
            });
        }

        /// <summary>
        /// Query devices
        /// </summary>
        /// <param name="query">
        /// Two types of query supported:
        /// 1. Serialized Clause list in JSON. Each clause includes three parts: key, operator and value
        /// 2. The "Where" clause of official IoTHub query string, except keyword "WHERE"
        /// </param>
        /// <param name="continuationToken">Continuation token. Not in use yet</param>
        /// <returns>List of devices</returns>
        public async Task<DeviceServiceListModel> GetListAsync(string query, string continuationToken)
        {
            if (!string.IsNullOrWhiteSpace(query))
            {
                // Try to translate clauses to query
                query = QueryConditionTranslator.ToQueryString(query);
            }

            // normally we need deviceTwins for all devices to show device list
            IEnumerable<Device> devices = await registry.GetDevicesAsync(MAX_GET_LIST);

            ResultWithContinuationToken<List<Twin>> twins = await GetTwinByQueryAsync(query, continuationToken, MAX_GET_LIST);

            // since deviceAsync does not support continuationToken for now, we need to ignore those devices which does not shown in twins
            return new DeviceServiceListModel(devices
                    .Where(d => twins.Result.Exists(t => d.Id == t.DeviceId))
                    .Select(azureDevice => new DeviceServiceModel(azureDevice, twins.Result.SingleOrDefault(t => t.DeviceId == azureDevice.Id), ioTHubHostName)),
                twins.ContinuationToken);
        }

        /// <summary>
        /// Query devices
        /// </summary>
        /// <returns>DeviceTwinName</returns>
        public async Task<DeviceTwinName> GetDeviceTwinNamesAsync()
        {
            DeviceServiceListModel content = await GetListAsync(string.Empty, string.Empty);

            return content.GetDeviceTwinNames();
        }

        public async Task<DeviceTwinServiceModel> GetDeviceTwinAsync(string id)
        {
            Twin content = await registry.GetTwinAsync(id);

            return new DeviceTwinServiceModel(content);
        }

        public async Task<DeviceServiceModel> GetAsync(string id)
        {
            Task<Device> device = registry.GetDeviceAsync(id);
            Task<Twin> twin = registry.GetTwinAsync(id);
            await Task.WhenAll(device, twin);


            if (device.Result == null)
            {
                throw new ResourceNotFoundException("The device doesn't exist.");
            }

            return new DeviceServiceModel(device.Result, twin.Result, ioTHubHostName);
        }

        public async Task<DeviceServiceModel> CreateAsync(DeviceServiceModel device)
        {
            // auto generate DeviceId, if missing
            if (string.IsNullOrEmpty(device.Id))
            {
                device.Id = Guid.NewGuid().ToString();
            }

            Device azureDevice = await registry.AddDeviceAsync(device.ToAzureModel());
            Twin azureTwin;
            if (device.Twin == null)
            {
                azureTwin = await registry.GetTwinAsync(device.Id);
            }
            else
            {
                azureTwin = await registry.UpdateTwinAsync(device.Id, device.Twin.ToAzureModel(), "*");
            }

            return new DeviceServiceModel(azureDevice, azureTwin, ioTHubHostName);
        }

        public async Task<DeviceServiceModel> CreateWithDesiredAsync(DeviceServiceModel device, string twinPatch)
        {
            // auto generate DeviceId, if missing
            if (string.IsNullOrEmpty(device.Id))
            {
                device.Id = Guid.NewGuid().ToString();
            }

            Device azureDevice = await registry.AddDeviceAsync(device.ToAzureModel());
            Twin azureTwin;

            azureTwin = await registry.UpdateTwinAsync(device.Id, twinPatch, "*");


            return new DeviceServiceModel(azureDevice, azureTwin, ioTHubHostName);
        }


        public async Task<bool> UpdateDeviceTwinAsync(DeviceServiceModel device)
        {
            Twin azureTwin = await registry.UpdateTwinAsync(device.Id, device.Twin.ToAzureModel(), "*");

            return azureTwin != null;
        }

        public async Task<bool> UpdateDeviceTwinAsync(string deviceId, string twinPatch)
        {
            Twin azureTwin = await registry.UpdateTwinAsync(deviceId, twinPatch, "*");

            return azureTwin != null;
        }

        /// <summary>
        /// We only support update twin
        /// </summary>
        /// <param name="device"></param>
        /// <param name="devicePropertyDelegate"></param>
        /// <returns></returns>
        public async Task<DeviceServiceModel> CreateOrUpdateAsync(DeviceServiceModel device, DevicePropertyDelegate devicePropertyDelegate)
        {
            // validate device module
            Device azureDevice = await registry.GetDeviceAsync(device.Id);
            if (azureDevice == null)
            {
                azureDevice = await registry.AddDeviceAsync(device.ToAzureModel());
            }

            Twin azureTwin;
            if (device.Twin == null)
            {
                azureTwin = await registry.GetTwinAsync(device.Id);
            }
            else
            {
                azureTwin = await registry.UpdateTwinAsync(device.Id, device.Twin.ToAzureModel(), device.Twin.ETag);

                // Update the deviceGroupFilter cache, no need to wait
                DevicePropertyServiceModel model = new DevicePropertyServiceModel();

                JToken tagRoot = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(device.Twin.Tags)) as JToken;
                if (tagRoot != null)
                {
                    model.Tags = new HashSet<string>(tagRoot.GetAllLeavesPath());
                }

                JToken reportedRoot = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(device.Twin.ReportedProperties)) as JToken;
                if (reportedRoot != null)
                {
                    model.Reported = new HashSet<string>(reportedRoot.GetAllLeavesPath());
                }
                Task<DevicePropertyServiceModel> unused = devicePropertyDelegate(model);
            }

            return new DeviceServiceModel(azureDevice, azureTwin, ioTHubHostName);
        }

        public async Task DeleteAsync(string id)
        {
            await registry.RemoveDeviceAsync(id);
        }

        /// <summary>
        /// Get twin result by query
        /// </summary>
        /// <param name="query">The query without prefix</param>
        /// <param name="continuationToken">The continuationToken</param>
        /// <param name="nubmerOfResult">The max result</param>
        /// <returns></returns>
        private async Task<ResultWithContinuationToken<List<Twin>>> GetTwinByQueryAsync(string query, string continuationToken, int nubmerOfResult)
        {
            query = string.IsNullOrEmpty(query) ? QUERY_PREFIX : $"{QUERY_PREFIX} where {query}";

            List<Twin> twins = new List<Twin>();

            IQuery twinQuery = registry.CreateQuery(query);

            QueryOptions options = new QueryOptions
            {
                ContinuationToken = continuationToken
            };

            while (twinQuery.HasMoreResults && twins.Count < nubmerOfResult)
            {
                QueryResponse<Twin> response = await twinQuery.GetNextAsTwinAsync(options);
                options.ContinuationToken = response.ContinuationToken;
                twins.AddRange(response);
            }

            return new ResultWithContinuationToken<List<Twin>>(twins, options.ContinuationToken);
        }

        private class ResultWithContinuationToken<T>
        {
            public T Result { get; private set; }

            public string ContinuationToken { get; private set; }

            public ResultWithContinuationToken(T queryResult, string continuationToken)
            {
                Result = queryResult;
                ContinuationToken = continuationToken;
            }
        }

        public async Task<MethodResultServiceModel> InvokeDeviceMethodAsync(string deviceId, MethodParameterServiceModel parameter)
        {
            CloudToDeviceMethodResult result = await serviceClient.InvokeDeviceMethodAsync(deviceId, parameter.ToAzureModel());
            return new MethodResultServiceModel(result);
        }
    }
}
