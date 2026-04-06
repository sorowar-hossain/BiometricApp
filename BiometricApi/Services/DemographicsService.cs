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
            // Step 1: Get Unique ID from SP
            var bioCode = await demographicsRepository.GetBioCodeAsync(demographic.OrgId);

            // Step 2: Create entity
            demographic.BiometricId = bioCode;

            // Step 3: Save using EF
            return await demographicsRepository.SaveAsync(demographic);
        }
    }
}
