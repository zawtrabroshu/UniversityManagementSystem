
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace University.Presentation.Api;

public class AuthApiService
{
    private readonly HttpClient httpClient;

    // Поточний токен авторизованого користувача
    public string? AccessToken { get; private set; }

    // Поточна роль авторизованого користувача
    public string? CurrentRole { get; private set; }

    public AuthApiService()
    {
        httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5000/")
        };
    }

    // Налаштування токена для захищених запитів
    private void SetAuthorizationHeader()
    {
        if (string.IsNullOrEmpty(AccessToken))
            throw new Exception("Користувач не авторизований.");

        httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer", AccessToken);
    }

    // Авторизація користувача та отримання токена
    public async Task<LoginResponse> LoginAsync(
        string username, string password)
    {
        httpClient.DefaultRequestHeaders.Authorization = null;

        var request = new LoginRequest
        {
            Username = username,
            Password = password
        };

        HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            "api/Users/login", request);

        string responseContent =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception("Помилка авторизації: " + responseContent);

        LoginResponse? result =
            JsonSerializer.Deserialize<LoginResponse>(
                responseContent,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        if (result == null)
            throw new Exception("Сервер повернув порожню відповідь.");

        AccessToken = result.Token;
        CurrentRole = result.Role;

        return result;
    }

    // Реєстрація нового користувача
    public async Task RegisterAsync(
        string username, string password, string role)
    {
        SetAuthorizationHeader();

        var request = new RegisterRequest
        {
            Username = username,
            Password = password,
            Role = role
        };

        HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            "api/Users/register", request);

        string responseContent =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception("Помилка реєстрації: " + responseContent);
    }

    // Отримання списку користувачів
    public async Task<List<UserResponse>> GetUsersAsync()
    {
        SetAuthorizationHeader();

        HttpResponseMessage response =
            await httpClient.GetAsync("api/Users");

        string responseContent =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception(
                "Помилка отримання користувачів: " + responseContent);

        List<UserResponse>? users =
            JsonSerializer.Deserialize<List<UserResponse>>(
                responseContent,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        return users ?? new List<UserResponse>();
    }

    // Зміна ролі користувача
    public async Task SetRoleAsync(string username, string role)
    {
        SetAuthorizationHeader();

        var request = new SetRoleRequest
        {
            Username = username,
            Role = role
        };

        HttpResponseMessage response = await httpClient.PutAsJsonAsync(
            "api/Users/role/set", request);

        string responseContent =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception("Помилка зміни ролі: " + responseContent);
    }

    // Зміна пароля поточного користувача
    public async Task ChangePasswordAsync(
        string oldPassword, string newPassword)
    {
        SetAuthorizationHeader();

        var request = new ChangePasswordRequest
        {
            OldPassword = oldPassword,
            NewPassword = newPassword
        };

        HttpResponseMessage response = await httpClient.PutAsJsonAsync(
            "api/Users/password/change", request);

        string responseContent =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception("Помилка зміни пароля: " + responseContent);
    }

    // Завершення сеансу користувача
    public void Logout()
    {
        AccessToken = null;
        CurrentRole = null;
        httpClient.DefaultRequestHeaders.Authorization = null;
    }
}

// Дані для запиту авторизації
public class LoginRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

// Дані, отримані від сервера після авторизації
public class LoginResponse
{
    public string Token { get; set; } = "";
    public string Role { get; set; } = "";
}

// Дані для реєстрації користувача
public class RegisterRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string Role { get; set; } = "";
}

// Дані користувачів, отримані від сервера
public class UserResponse
{
    public string UserName { get; set; } = "";
    public string Role { get; set; } = "";
}

// Дані для зміни ролі користувача
public class SetRoleRequest
{
    public string Username { get; set; } = "";
    public string Role { get; set; } = "";
}

// Дані для зміни пароля
public class ChangePasswordRequest
{
    public string OldPassword { get; set; } = "";
    public string NewPassword { get; set; } = "";
}
