using Essentials.Business.Contracts;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace ProcessLowBatteryRequest
{
    public static class ProcessLowBatteryRequest
    {
        [FunctionName("ProcessLowBatteryRequest")]
        public static async Task Run([ServiceBusTrigger("lowbatteryrequest",
            Connection = "ServiceBusConnectionString")]string myQueueItem,
            ILogger log, ExecutionContext context)
        {
            try
            {
                ServiceCollection serviceCollection = new ServiceCollection();
                serviceCollection.AddDependencies();
                ServiceProvider Container = serviceCollection.BuildServiceProvider();
                IServiceScopeFactory serviceScopeFactory = Container.GetRequiredService<IServiceScopeFactory>();
                using (IServiceScope scope = serviceScopeFactory.CreateScope())
                {
                    Essentials.Business.Contracts.LowBatteryRequest lowBatteryRequest = JsonConvert.DeserializeObject<Essentials.Business.Contracts.LowBatteryRequest>(myQueueItem);
                    if (lowBatteryRequest != null)
                    {
                        ILockService lockService = scope.ServiceProvider.GetService<ILockService>();

                        await lockService.LowBatteryAsync(lowBatteryRequest.LockId,
                            new Essentials.Business.Contracts.LowBatteryRequest
                            {
                                LockId = lowBatteryRequest.LockId,
                                BatteryStatus = lowBatteryRequest.BatteryStatus
                            });
                    }
                }
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}
