@using System.Net.Http.Headers
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Blazored.LocalStorage;

namespace MyFit.Client.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;

    public ApiService(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
    }

    public async Task SetAuthToken(string token)
    {
        await _localStorage.SetItemAsync("authToken", token);
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<string?> GetAuthToken()
    {
        return await _localStorage.GetItemAsync<string>("authToken");
    }

    public async Task RemoveAuthToken()
    {
        await _localStorage.RemoveItemAsync("authToken");
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        await EnsureAuthHeader();
        var response = await _httpClient.GetAsync($"/api/{endpoint}");
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<T>();
        }
        
        return default;
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        await EnsureAuthHeader();
        var response = await _httpClient.PostAsJsonAsync($"/api/{endpoint}", data);
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<TResponse>();
        }
        
        var error = await response.Content.ReadAsStringAsync();
        throw new Exception($"API Error: {error}");
    }

    private async Task EnsureAuthHeader()
    {
        if (_httpClient.DefaultRequestHeaders.Authorization == null)
        {
            var token = await GetAuthToken();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
}
