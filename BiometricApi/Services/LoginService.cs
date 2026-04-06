using BiometricApi.Data;
using BiometricApi.Entities;
using BiometricApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BiometricApi.Services
{
    public class LoginService
    {
        private readonly AppDbContext context;

        public LoginService(AppDbContext context)
        {
            this.context = context;
        }

        public async Task<UserLoginViewModel?> ValidateCredentialsAsync(string userName, string password)
        {
            var user = await (from u in context.Users
                              join o in context.Organizations
                                  on u.OrgId equals o.OrgId
                              where u.UserName == userName
                                    && u.IsActive
                                    && u.Password == password
                                    && o.IsActive
                              select new UserLoginViewModel
                              {
                                  UserId = u.UserId,
                                  OrgId = u.OrgId,
                                  OrgCode = o.Code,
                                  UserName = u.UserName
                              }).FirstOrDefaultAsync();

            if (user == null)
                return null;

            // if password store in hash format
            //if (!PasswordHelper.VerifyPassword(password, user.Password))
            //    return null;

            return user;
        }
    }
}
