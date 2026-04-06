using BiometricApi.Data;
using BiometricApi.Entities;
using BiometricApi.IRepository;
using BiometricApi.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Cryptography;

namespace BiometricApi.Repository
{
    public class DemographicsRepository : IDemographicsRepository
    {
        private readonly AppDbContext context;
        private readonly IConfiguration config;

        public DemographicsRepository(AppDbContext context, IConfiguration config)
        {
            this.context = context;
            this.config = config;
        }

        public async Task<string> GetBioCodeAsync(int orgId) 
        {
            BiometricViewModel biometricViewModel = new BiometricViewModel();
            using var conn = new SqlConnection(config.GetConnectionString("connectionString"));

            var result = await conn.QueryFirstOrDefaultAsync<string>(
                "sp_GetBioUniqueId",
                new { OrgId = orgId },
                commandType: CommandType.StoredProcedure
            );

            return result;
        }

        public async Task<bool> SaveAsync(Demographic entity)
        {
            context.Add(entity);
            var result = await context.SaveChangesAsync();
            return result > 0;
        }
    }
}
