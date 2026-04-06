using BiometricApi.Entities;
using BiometricApi.Models;

namespace BiometricApi.IRepository
{
    public interface IDemographicsRepository
    {
        Task<string> GetBioCodeAsync(int orgId);  
        Task<bool> SaveAsync(Demographic entity);
    }
}
