using AutoMapper;
using Essentials.Business.Contracts;
using Essentials.Business.Runtime;

namespace Essentials.Business.Services
{
    public partial class LockService : ILockService
    {
        private readonly IServicesConfig _servicesConfig;
        private readonly IMapper _mapper;

        public LockService(IServicesConfig servicesConfig,
            IMapper mapper)
        {
            _servicesConfig = servicesConfig;
            _mapper = mapper;
        }

    }
}
