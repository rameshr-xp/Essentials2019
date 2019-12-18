using System.Threading.Tasks;

namespace Essentials.Business.Contracts
{
    public class LowBatteryRequest
    {
        public long LockId { get; set; }
        public int BatteryStatus { get; set; }
    }

    public interface ILockService
    {
        Task<bool> LowBatteryAsync(long lockId, LowBatteryRequest lowBatteryRequest);
    }
}
