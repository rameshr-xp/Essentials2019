using AzureFunctions.Common.Models;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace SimulatedDevice
{
    internal class SimulatedDevice
    {
        private static DeviceClient s_deviceClient;

        private static string apiBaseUrl = "";
        private static string deviceName = "";
        private static long lockId = 4;
        private static string s_connectionString = "HostName=iothub-essentials2019.azure-devices.net;DeviceId=essentials2019;SharedAccessKey=W8F1G7TukTMX2GgE6pw4p8WoREMUku0Zr/ioqNYmuPY=";


        private static Task<MethodResponse> UpdateSetting(MethodRequest methodRequest, object userContext)
        {

            string data = Encoding.UTF8.GetString(methodRequest.Data);
            Console.WriteLine("");
            Console.WriteLine("UpdateSetting method called." + methodRequest.DataAsJson);
            Console.WriteLine("");
            string result = "{\"result\":\"Executed direct method: " + methodRequest.Name + "\"}";
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
        }

        private static Task<MethodResponse> Ping(MethodRequest methodRequest, object userContext)
        {

            string data = Encoding.UTF8.GetString(methodRequest.Data);
            Console.WriteLine("");
            Console.WriteLine("Ping method called." + methodRequest.DataAsJson);
            Console.WriteLine("");
            string result = "{\"result\":\"Executed direct method: " + methodRequest.Name + "\"}";
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
        }

        // Async method to send simulated telemetry
        private static async void UpdateLockBoxStatusReportedPropertiesAsync(string lockStatus)
        {
            TwinCollection reportedProperties, lockBoxStatus;
            reportedProperties = new TwinCollection();
            lockBoxStatus = new TwinCollection
            {
                ["wifiStatus"] = "Connected",
                ["batteryStatus"] = "90%",
                ["lockStatus"] = lockStatus
            };

            reportedProperties["lockBoxStatus"] = lockBoxStatus;

            await s_deviceClient.UpdateReportedPropertiesAsync(reportedProperties);
        }

        private static void Main(string[] args)
        {
            Console.WriteLine($"Started Simulated device({deviceName}). Ctrl-C to exit.\n");

            // Connect to the IoT hub using the MQTT protocol
            s_deviceClient = DeviceClient.CreateFromConnectionString(s_connectionString, Microsoft.Azure.Devices.Client.TransportType.Mqtt);

            UpdateLockBoxStatusReportedPropertiesAsync("Locked");

            // Create a handler for the direct method call
            s_deviceClient.SetMethodHandlerAsync("UpdateSetting", UpdateSetting, null).Wait();
            s_deviceClient.SetMethodHandlerAsync("Ping", Ping, null).Wait();

            string res = "n";
            do
            {
                Console.WriteLine("Send device to cloud message?");
                Console.WriteLine("Menu:");
                Console.WriteLine("1. Low Battery");
                Console.WriteLine("");
                Console.Write("Enter option:");

                string input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                        SendLowBatteryRequestAsync();
                        break;
                    default:
                        Console.WriteLine("Invalid input");
                        break;
                }

                Console.WriteLine("Do you want to send another request? y/n");
                res = Console.ReadLine();
            }
            while (res == "y");
            Console.ReadLine();
        }

        private static async Task SendLowBatteryRequestAsync()
        {
            try
            {
                // Create a new message to send to the queue.
                var request = new 
                {
                    LockID = lockId,
                    BatteryStatus = 9
                };

                Message message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request)));
                message.Properties.Add("type", DeviceToCloudMessageType.LowBatteryRequest.ToString());
                await s_deviceClient.SendEventAsync(message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("!!!! " + ex.Message);
            }
        }
    }
}
