Markdown

# JwtAuthLibrary Documentation

`JwtAuthLibrary` is a .NET Standard library designed to provide essential and reusable functionalities for handling JSON Web Tokens (JWT) in your applications. This library aims to simplify the process of creating Access Tokens and Refresh Tokens, as well as providing the necessary tools for validating tokens and extracting information from them.

## Key Features

* **Access Token Creation:** Generate signed Access Tokens based on user claims and JWT settings.
* **Refresh Token Generation:** Generate strong, cryptographically random Refresh Tokens.
* **Claims Extraction:** Ability to extract claims from an expired (but validly signed) Access Token to facilitate the token refresh process.
* **Easy Configuration:** Provides an Extension Method to simplify the setup of JWT services within the Dependency Injection system in ASP.NET Core.
* **Separation of Concerns:** Separates token creation logic from your application's core business logic and database interactions.

## Installation

To add the library to your project, you can use the NuGet Package Manager.

**Via NuGet Package Manager Console:**

```powershell
Install-Package JwtAuthLibrary
Via .NET CLI:

Bash

dotnet add package JwtAuthLibrary
Configuration
The library relies on JWT settings located in your project's configuration file (typically appsettings.json). Your appsettings.json file must contain a Jwt section in the following format:

JSON

{
  "Jwt": {
    "Key": "YOUR_SUPER_SECRET_KEY_AT_LEAST_32_CHARS_LONG",
    "Issuer": "[https://yourdomain.com](https://yourdomain.com)",
    "Audience": "[https://yourdomain.com](https://yourdomain.com)",
    "AccessTokenExpiryMinutes": 15, // Optional: Access Token validity duration in minutes (Default: 15)
    "RefreshTokenExpiryDays": 7    // Optional: Refresh Token validity duration in days (Default: 7)
  }
}
Important Configuration Notes:

Key: This is the secret key used to sign the JWTs. Never store it directly in appsettings.json in production environments. Use environment variables, Azure Key Vault, AWS Secrets Manager, or any secure secret management solution.

Issuer: The entity that issued the token (typically your application's URL).

Audience: The intended recipient of the token (typically your application's URL).

AccessTokenExpiryMinutes: The duration for which the Access Token remains valid. It is recommended to keep this short (e.g., 5-15 minutes) to mitigate the risk of token theft.

RefreshTokenExpiryDays: The duration for which the Refresh Token remains valid. This can be longer (e.g., 7 days or more) to allow users to stay logged in for extended periods without re-authenticating.

How to Use
1. Registering Services in Program.cs (for ASP.NET Core projects)
The library comes with an AddJwtAuthLibrary extension method to simplify the process of registering JWT services within the ASP.NET Core Dependency Injection system.

C#

// Program.cs
using JwtAuthLibrary.Extensions; // Make sure to add this using directive

var builder = WebApplication.CreateBuilder(args);

// ... other services (e.g., DbContext)

// Register JWT services from the library
builder.Services.AddJwtAuthLibrary(builder.Configuration);

// ... Controller services or Minimal APIs

var app = builder.Build();

// ... configure the pipeline (UseAuthentication, UseAuthorization)
app.UseAuthentication();
app.UseAuthorization();
// ...

app.Run();
Note: app.UseAuthentication(); must be placed before app.UseAuthorization(); in the HTTP request pipeline.

2. Using IJwtAuthService
You can inject IJwtAuthService into any service or controller that needs to create or validate tokens.

Example of injecting IJwtAuthService into a service:
C#

// UserService.cs (or any other service)
using JwtAuthLibrary.Services; // Make sure to add this using directive
using System.Security.Claims; // For ClaimTypes
using JwtAuthLibrary.Models.DTOs; // For TokenResponseDto, RefreshTokenRequestDto

public class UserService
{
    private readonly IJwtAuthService _jwtAuthService;

    public UserService(IJwtAuthService jwtAuthService)
    {
        _jwtAuthService = jwtAuthService;
    }

    // ...

    public async Task<TokenResponseDto?> LoginAsync(string email, string password)
    {
        // ... (user and password validation)

        // Example: Create claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        // Use IJwtAuthService to create an Access Token
        var accessToken = _jwtAuthService.CreateAccessToken(claims);

        // Use IJwtAuthService to create a Refresh Token
        var refreshToken = _jwtAuthService.GenerateRefreshToken();

        // **Important:** The Refresh Token must be saved in the database with an expiry date
        // (this part is still your application's responsibility, not the library's)
        // await SaveRefreshTokenToDatabase(user.ID, refreshToken, DateTime.UtcNow.AddDays(7));

        return new TokenResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto request)
    {
        // **Important:** The Refresh Token must be validated against the database here
        // (this part is still your application's responsibility)
        // var user = await GetUserByRefreshTokenFromDatabase(request.UserId, request.RefreshToken);
        // if (user == null) return null;

        // Extract claims from the expired Access Token (if you want to reuse them)
        // ClaimsPrincipal? principal = _jwtAuthService.GetPrincipalFromExpiredToken(request.AccessToken);
        // var userIdFromToken = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // if (userIdFromToken == null || !Guid.TryParse(userIdFromToken, out Guid parsedUserId) || parsedUserId != request.UserId)
        // {
        //     return null; // Invalid token or mismatch with user
        // }

        // Example: Re-create claims (or use extracted claims)
        // If you're using GetPrincipalFromExpiredToken, you can get claims from the principal:
        // var claims = principal?.Claims;
        // Otherwise, create new claims from the user object retrieved from the database:
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        // Create a new Access Token
        var newAccessToken = _jwtAuthService.CreateAccessToken(claims);

        // Create a new Refresh Token
        var newRefreshToken = _jwtAuthService.GenerateRefreshToken();

        // **Important:** The new Refresh Token must be updated in the database
        // await SaveRefreshTokenToDatabase(user.ID, newRefreshToken, DateTime.UtcNow.AddDays(7));

        return new TokenResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        };
    }
}
3. Used DTOs
TokenResponseDto: Used to return the Access Token and Refresh Token to the client after login or token refresh.

C#

namespace JwtAuthLibrary.Models.DTOs
{
    public class TokenResponseDto
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
    }
}
RefreshTokenRequestDto: Used to receive the token refresh request from the client.

C#

using System;
using System.ComponentModel.DataAnnotations;

namespace JwtAuthLibrary.Models.DTOs
{
    public class RefreshTokenRequestDto
    {
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public required string RefreshToken { get; set; }
    }
}
Core Concepts
JWT (JSON Web Token): A secure way to represent claims between two parties. It consists of three parts separated by dots: Header, Payload, Signature.

Access Token: The actual token sent with every authenticated API request. It has a short expiry time (usually minutes).

Refresh Token: A long-lived token used to obtain a new Access Token when the current Access Token expires, without requiring the user to log in again.

Claims: Information about the entity (typically the user) for whom the token was issued, such as user ID, email, roles, etc.

Important Security Considerations
JWT Secret Key (Jwt:Key):

Never store it directly in appsettings.json in production environments. Use environment variables, Azure Key Vault, AWS Secrets Manager, or any secure secret management solution.

It must be very strong and truly random. Use a cryptographic key generator (like the one you provided previously).

Password Hashing:

This library does not handle password hashing. You should always hash passwords before storing them in the database (e.g., using BCrypt or Argon2) and verify the hash during login.

Refresh Token Management:

The library does not store or manage Refresh Tokens in the database. This is your application's responsibility.

Refresh Tokens should be stored in a secure database and linked to the user.

Refresh Tokens should be revoked upon user logout, upon detection of suspicious activity, or upon their expiration.

HTTPS: Always ensure your application uses HTTPS to protect tokens during transmission.

Error Handling: Your application should properly handle scenarios where a token is invalid or expired.
