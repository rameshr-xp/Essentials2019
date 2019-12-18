namespace AzureFunctions.Common.Models
{
    public enum DeviceToCloudMessageType
    {
        LowBatteryRequest = 1,
    }
    public class DeviceToCloudMessage
    {
        public object Payload { get; set; }
    }

    public class LowBatteryRequest
    {
        public long LockId { get; set; }
        public int BatteryStatus { get; set; }
    }

    public class CarrierOtpResponse
    {
        public long LockId { get; set; }
    }
}
