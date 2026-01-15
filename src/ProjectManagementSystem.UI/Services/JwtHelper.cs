using System.Security.Claims;
using System.Text.Json;

namespace ProjectManagementSystem.UI.Services;

public static class JwtHelper
{
    public static string? GetRoleFromToken(string token)
    {
        try
        {
            // JWT tokens have 3 parts separated by dots: header.payload.signature
            var parts = token.Split('.');
            if (parts.Length != 3)
                return null;

            // Decode the payload (second part)
            var payload = parts[1];
            
            // Add padding if needed for base64 decoding
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            var payloadBytes = Convert.FromBase64String(payload);
            var payloadJson = System.Text.Encoding.UTF8.GetString(payloadBytes);
            var payloadDoc = JsonDocument.Parse(payloadJson);

            // Try to get role from claims
            if (payloadDoc.RootElement.TryGetProperty("role", out var roleElement))
            {
                return roleElement.GetString();
            }

            // Try alternative claim names
            if (payloadDoc.RootElement.TryGetProperty("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", out var roleClaim))
            {
                return roleClaim.GetString();
            }

            // Check roles array
            if (payloadDoc.RootElement.TryGetProperty("roles", out var rolesElement))
            {
                if (rolesElement.ValueKind == JsonValueKind.Array && rolesElement.GetArrayLength() > 0)
                {
                    return rolesElement[0].GetString();
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}
