using AutoMapper;
using AzureIotHub.Services;
using Essentials.Business.Contracts;
using Essentials.Business.Runtime;
using Essentials.Business.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ProcessLowBatteryRequest
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDependencies(this IServiceCollection services)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                    .SetBasePath(Environment.CurrentDirectory)
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();

            ServicesConfig servicesConfig = new ServicesConfig
            {
                IoTHubConnectionString = config.GetValue<string>("AzureIOTHubIoTHubConnString"),
                IoTHubStorageConnectionString = config.GetValue<string>("AzureStorageConnectionString"),
            };


            services.AddSingleton<AzureIotHub.Services.Runtime.IServicesConfig>(servicesConfig);
            services.AddSingleton<Essentials.Business.Runtime.IServicesConfig>(servicesConfig);
            services.AddScoped<IDeviceRegistryService, DeviceRegistryService>();
            services.AddScoped<IDeviceConfigurationService, DeviceConfigurationService>();
            services.AddScoped<ILockService, LockService>();
            services.AddAutoMapper(typeof(ServiceCollectionExtensions));

            return services;
        }
    }
}
