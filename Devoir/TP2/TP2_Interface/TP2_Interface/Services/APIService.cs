using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TP2_API.DTOs;
using TP2_Interface.Models;

public class APIService
{
    private readonly HttpClient _httpClient;

    public APIService()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("http://localhost:5051/"); 
    }

    public async Task<UserDto> CreateUserAsync(UserCreationDto userDto)
    {
        var content = new StringContent(JsonConvert.SerializeObject(userDto), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("api/users", content);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<UserDto>(responseContent);
    }

    public async Task<LoginResponse> LoginAsync(LoginDto loginDto)
    {
        var content = new StringContent(JsonConvert.SerializeObject(loginDto), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("api/users/login", content);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<LoginResponse>(responseContent);
    }

    public async Task<bool> PromoteToAdminAsync(string username, string requesterUsername)
    {
        var content = new StringContent(JsonConvert.SerializeObject(requesterUsername), Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"api/users/promote/{username}", content);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var response = await _httpClient.GetAsync("api/users/all");
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<UserDto>>(responseContent);
        }

        return new List<UserDto>(); 
    }

    public async Task<bool> DeleteUserAsync(int userId, string requesterUsername)
    {
        var requestUri = $"api/users/{userId}";
        var content = new StringContent(JsonConvert.SerializeObject(requesterUsername), Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            RequestUri = new Uri(_httpClient.BaseAddress, requestUri),
            Content = content
        };

        var response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }


    

    public async Task<UserDto> GetUserByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"api/users/{id}");
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<UserDto>(responseContent);
    }

}

