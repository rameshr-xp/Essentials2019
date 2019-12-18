using AzureIotHub.Services.Helpers;
using AzureIotHub.Services.Runtime;
using Microsoft.Azure.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureIotHub.Services
{
    public interface IDeviceConfigurationService
    {
        Task<bool> AddDeviceConfiguration(string configurationId,
            string deviceContentKey,
            object deviceContentValue,
            Dictionary<string, string> metricsDictionary,
            string targetCondition);

        Task DeleteConfiguration(string configurationId);

        Task<List<Configuration>> GetConfigurations(int count);
    }
    public class DeviceConfigurationService : IDeviceConfigurationService
    {
        private RegistryManager _registryManager;

        public DeviceConfigurationService(
            IServicesConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            IoTHubConnectionHelper.CreateUsingHubConnectionString(config.IoTHubConnectionString, (conn) =>
                    {
                        _registryManager = RegistryManager.CreateFromConnectionString(conn);
                    });
        }

        public async Task<bool> AddDeviceConfiguration(string configurationId,
            string deviceContentKey,
            object deviceContentValue,
            Dictionary<string, string> metricsDictionary,
            string targetCondition)
        {
            Configuration configuration = new Configuration(configurationId);

            CreateDeviceContent(configuration, configurationId,
                deviceContentKey, deviceContentValue);

            CreateMetrics(configuration, configurationId, metricsDictionary);

            CreateTargetCondition(configuration, configurationId, targetCondition);

            Configuration configurationNew = await _registryManager.AddConfigurationAsync(configuration).ConfigureAwait(false);

            if (configurationNew != null)
            {
                List<Configuration> configurations = await GetConfigurations(2);

                foreach (Configuration item in configurations)
                {
                    if (item.Id != configurationNew.Id)
                    {
                        await DeleteConfiguration(item.Id);
                    }
                }
            }
            Console.WriteLine("Configuration added, id: " + configurationId);

            return true;
        }

        private void CreateDeviceContent(Configuration configuration, string configurationId,
            string deviceContentKey, object deviceContentValue)
        {
            configuration.Content = new ConfigurationContent
            {
                DeviceContent = new Dictionary<string, object>()
            };
            configuration.Content.DeviceContent[deviceContentKey] = deviceContentValue;
        }

        private void CreateMetrics(Configuration configuration,
            string configurationId,
            Dictionary<string, string> metricsDictionary)
        {
            foreach (KeyValuePair<string, string> item in metricsDictionary)
            {
                configuration.Metrics.Queries.Add(item.Key, item.Value);
            }
            configuration.TargetCondition = "*";
        }

        private void CreateTargetCondition(Configuration configuration,
            string configurationId,
            string targetCondition)
        {
            configuration.TargetCondition = targetCondition;
            configuration.Priority = 20;
        }

        public async Task DeleteConfiguration(string configurationId)
        {
            await _registryManager.RemoveConfigurationAsync(configurationId).ConfigureAwait(false);

            Console.WriteLine("Configuration deleted, id: " + configurationId);
        }

        public async Task<List<Configuration>> GetConfigurations(int count)
        {
            IEnumerable<Configuration> configurations = await _registryManager.GetConfigurationsAsync(count).ConfigureAwait(false);

            Console.WriteLine("Configurations received");

            return await Task.FromResult(configurations.ToList());
        }

        public void PrintConfiguration(Configuration configuration)
        {
            Console.WriteLine("Configuration Id: " + configuration.Id);
            Console.WriteLine("Configuration SchemaVersion: " + configuration.SchemaVersion);

            Console.WriteLine("Configuration Labels: " + configuration.Labels);

            PrintContent(configuration.ContentType, configuration.Content);

            Console.WriteLine("Configuration TargetCondition: " + configuration.TargetCondition);
            Console.WriteLine("Configuration CreatedTimeUtc: " + configuration.CreatedTimeUtc);
            Console.WriteLine("Configuration LastUpdatedTimeUtc: " + configuration.LastUpdatedTimeUtc);

            Console.WriteLine("Configuration Priority: " + configuration.Priority);

            PrintConfigurationMetrics(configuration.SystemMetrics, "SystemMetrics");
            PrintConfigurationMetrics(configuration.Metrics, "Metrics");

            Console.WriteLine("Configuration ETag: " + configuration.ETag);
            Console.WriteLine("------------------------------------------------------------");
        }

        private void PrintContent(string contentType, ConfigurationContent configurationContent)
        {
            Console.WriteLine($"Configuration Content [type = {contentType}]");

            Console.WriteLine("ModuleContent:");
            foreach (string modulesContentKey in configurationContent.ModulesContent.Keys)
            {
                foreach (string key in configurationContent.ModulesContent[modulesContentKey].Keys)
                {
                    Console.WriteLine($"\t\t{key} = {configurationContent.ModulesContent[modulesContentKey][key]}");
                }
            }

            Console.WriteLine("DeviceContent:");
            foreach (string key in configurationContent.DeviceContent.Keys)
            {
                Console.WriteLine($"\t{key} = {configurationContent.DeviceContent[key]}");
            }
        }

        private void PrintConfigurationMetrics(ConfigurationMetrics metrics, string title)
        {
            Console.WriteLine($"{title} Results: ({metrics.Results.Count})");
            foreach (string key in metrics.Results.Keys)
            {
                Console.WriteLine($"\t{key} = {metrics.Results[key]}");
            }

            Console.WriteLine($"{title} Queries: ({metrics.Queries.Count})");
            foreach (string key in metrics.Queries.Keys)
            {
                Console.WriteLine($"\t{key} = {metrics.Queries[key]}");
            }
        }
    }
}
