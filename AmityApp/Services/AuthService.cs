using AmityApp.Shared.Dtos;
using System.Text.Json;

namespace AmityApp.Services;

public class AuthService
{
    private const string UserDataKey = "udata";

    public AuthService()
    {
        Initialize();
    }
    public string? Token { get; set; }
    public LoggedInUser? User { get; set; }

    public bool IsLoggedIn => User is not null && User.Id != default && !string.IsNullOrWhiteSpace(Token);

    public void Login(LoginResponseDto loginResponseDto)
    {
        User = loginResponseDto.User;
        Token = loginResponseDto.Token;
        Preferences.Default.Set(UserDataKey, JsonSerializer.Serialize(loginResponseDto));
    }

    public void Logout()
    {
        (User, Token) = (null, null);
        Preferences.Default.Remove(UserDataKey);
    }

    public void Initialize()
    {
        var udata = Preferences.Default.Get<string?>(UserDataKey, null);
        if (!string.IsNullOrWhiteSpace(udata))
        {
            var loginResponse = JsonSerializer.Deserialize<LoginResponseDto>(udata);
            if (loginResponse != null && loginResponse.User is not null && loginResponse.User.Id != default)
            {
                User = loginResponse.User;
                Token = loginResponse.Token;
            }
            else
            {
                Preferences.Default.Remove(UserDataKey);
            }
        }
    }
}