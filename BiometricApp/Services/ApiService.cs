using BiometricApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace BiometricApp.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        // Accept HttpClient from DI
        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<string>> GetUsersAsync()
        {
            // Will automatically prepend BaseAddress from DI
            return await _httpClient.GetFromJsonAsync<List<string>>("api/users");
        }


        public async Task<UserLoginResponse?> LoginAsync(string userName, string password)
        {
            var loginRequest = new LoginRequest
            {
                UserName = userName,
                Password = password
            };

            // POST to your API endpoint
            var response = await _httpClient.PostAsJsonAsync("api/Auth/login", loginRequest);

            if (!response.IsSuccessStatusCode)
            {
                // Optionally read error message
                var errorMsg = await response.Content.ReadAsStringAsync();
                return null;
            }

            var user = await response.Content.ReadFromJsonAsync<UserLoginResponse>();
            return user;
        
        }

    }
}
