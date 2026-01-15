using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace ProjectManagementSystem.UI.Services;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IAuthService _authService;

    public CustomAuthenticationStateProvider(IAuthService authService)
    {
        _authService = authService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var isAuthenticated = await _authService.IsAuthenticatedAsync();
        
        if (!isAuthenticated)
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var token = await _authService.GetTokenAsync();
        var role = await _authService.GetUserRoleAsync();
        var userDto = await _authService.GetUserAsync();

        var claims = new List<Claim>();
        
        if (!string.IsNullOrEmpty(token))
        {
            claims.Add(new Claim(ClaimTypes.Authentication, token));
        }
        
        if (!string.IsNullOrEmpty(role))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        
        if (userDto != null)
        {
            if (!string.IsNullOrEmpty(userDto.Email))
            {
                claims.Add(new Claim(ClaimTypes.Name, userDto.Email));
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userDto.Id));
            }
            if (!string.IsNullOrEmpty(userDto.FirstName))
            {
                claims.Add(new Claim(ClaimTypes.GivenName, userDto.FirstName));
            }
            if (!string.IsNullOrEmpty(userDto.LastName))
            {
                claims.Add(new Claim(ClaimTypes.Surname, userDto.LastName));
            }
        }

        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        return new AuthenticationState(user);
    }

    public void NotifyUserAuthentication()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void NotifyUserLogout()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
