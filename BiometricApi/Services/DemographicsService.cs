using BiometricApi.Entities;
using BiometricApi.IRepository;

namespace BiometricApi.Services
{
    public class DemographicsService
    {
        private readonly IDemographicsRepository demographicsRepository;

        public DemographicsService(IDemographicsRepository repo)
        {
            demographicsRepository = repo;
        }

        public async Task<bool> CreateAsync(Demographic demographic)
        {
            return await demographicsRepository.SaveAsync(demographic);
        }

        public async Task<string> GetPersonUniqueId(int orgId)
        {
            // Get Unique ID from SP
            string id = await demographicsRepository.GetPersonUniqueIdAsync(orgId); 
            return id;

        }

    }
}
