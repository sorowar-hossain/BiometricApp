using BiometricApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiometricApp.Services
{
    public class UserSessionService
    {
        public UserLoginResponse User { get; private set; } = new();

        public event Action? OnChange;

        public void SetUser(UserLoginResponse user)
        {
            User = user;
            NotifyStateChanged();
        }

        public void Update(Action<UserLoginResponse> updateAction)
        {
            updateAction(User);
            NotifyStateChanged();
        }

        private void NotifyStateChanged()
        {
            OnChange?.Invoke();
        }

        public void Clear()
        {
            User = new UserLoginResponse();
            NotifyStateChanged();
        }
    }
}
