using System.Net.Http;
using System.Text;
using System.Text.Json;
using InstallyAPI.Models;
using InstallyApp.Models;

namespace InstallyApp.DataServices
{
    public class RestDataService : IRestDataService
    {
        private readonly string _baseAddress;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonSeriarizerOptions;
        private readonly string _url;

        public RestDataService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            _baseAddress = "http://localhost:5272";
            _url = $"{_baseAddress}/api";

            _jsonSeriarizerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task AddUserAsync(UserEntity user)
        {
            try
            {
                var jsonUser = JsonSerializer.Serialize(user, _jsonSeriarizerOptions);
                var content = new StringContent(jsonUser, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_url}/user", content);

                if (response.IsSuccessStatusCode)
                    Console.WriteLine("User successfully created");
                else
                    Console.WriteLine("----No Http 2xx response----");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"----Exception throw: {ex.Message}----");
            }
        }

        public async Task<List<UserEntity>> GetAllUsersAsync()
        {
            List<UserEntity> users = new();

            try
            {
                var response = await _httpClient.GetAsync($"{_url}/user");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    users = JsonSerializer.Deserialize<List<UserEntity>>(content, _jsonSeriarizerOptions);
                }
                else
                {
                    Console.WriteLine("----No Http 2xx response----");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"----Exception throw: {ex.Message}----");
            }

            return users;
        }

        public async Task UpdateUserAsync(UserEntity user)
        {
            try
            {
                var jsonUser = JsonSerializer.Serialize(user, _jsonSeriarizerOptions);
                var content = new StringContent(jsonUser, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{_url}/user/{user.Guid}", content);

                if (response.IsSuccessStatusCode)
                    Console.WriteLine("User successfully updated");
                else
                    Console.WriteLine("----No Http 2xx response----");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"----Exception throw: {ex.Message}----");
            }
        }

        public async Task DeleteUserAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_url}/user/{id}");

                if (response.IsSuccessStatusCode)
                    Console.WriteLine("User successfully deleted");
                else
                    Console.WriteLine("----No Http 2xx response----");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"----Exception throw: {ex.Message}----");
            }
        }
    }
}