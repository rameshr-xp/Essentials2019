using Essentials.Business.Contracts;
using System.Threading.Tasks;

namespace Essentials.Business.Services
{
    public partial class LockService
    {
        

        public async Task<bool> LowBatteryAsync(long lockId, LowBatteryRequest lowBatteryRequest)
        {
            return true;
        }
        
    }
}
