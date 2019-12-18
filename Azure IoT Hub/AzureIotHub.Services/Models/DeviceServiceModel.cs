

using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using System;

namespace AzureIotHub.Services.Models
{
    public class DeviceServiceModel
    {
        public string Etag { get; set; }
        public string Id { get; set; }
        public int C2DMessageCount { get; set; }
        public DateTime LastActivity { get; set; }
        public bool Connected { get; set; }
        public bool Enabled { get; set; }
        public DateTime LastStatusUpdated { get; set; }
        public DeviceTwinServiceModel Twin { get; set; }
        public string IoTHubHostName { get; set; }
        public AuthenticationMechanismServiceModel Authentication { get; set; }

        public DeviceServiceModel(
            string etag,
            string id,
            int c2DMessageCount,
            DateTime lastActivity,
            bool connected,
            bool enabled,
            DateTime lastStatusUpdated,
            DeviceTwinServiceModel twin,
            AuthenticationMechanismServiceModel authentication,
            string ioTHubHostName)
        {
            Etag = etag;
            Id = id;
            C2DMessageCount = c2DMessageCount;
            LastActivity = lastActivity;
            Connected = connected;
            Enabled = enabled;
            LastStatusUpdated = lastStatusUpdated;
            Twin = twin;
            IoTHubHostName = ioTHubHostName;
            Authentication = authentication;
        }

        public DeviceServiceModel(Device azureDevice, DeviceTwinServiceModel twin, string ioTHubHostName) :
            this(
                etag: azureDevice.ETag,
                id: azureDevice.Id,
                c2DMessageCount: azureDevice.CloudToDeviceMessageCount,
                lastActivity: azureDevice.LastActivityTime,
                connected: azureDevice.ConnectionState.Equals(DeviceConnectionState.Connected),
                enabled: azureDevice.Status.Equals(DeviceStatus.Enabled),
                lastStatusUpdated: azureDevice.StatusUpdatedTime,
                twin: twin,
                ioTHubHostName: ioTHubHostName,
                authentication: new AuthenticationMechanismServiceModel(azureDevice.Authentication))
        {
        }

        public DeviceServiceModel(Device azureDevice, Twin azureTwin, string ioTHubHostName) :
            this(azureDevice, new DeviceTwinServiceModel(azureTwin), ioTHubHostName)
        {
        }

        public Device ToAzureModel(bool ignoreEtag = true)
        {
            Device device = new Device(Id)
            {
                ETag = ignoreEtag ? null : Etag,
                Status = Enabled ? DeviceStatus.Enabled : DeviceStatus.Disabled,
                Authentication = Authentication == null ? null : Authentication.ToAzureModel()
            };

            return device;
        }
    }
}
