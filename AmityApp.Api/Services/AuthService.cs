using AmityApp.Api.Data;
using AmityApp.Api.Data.Entities;
using AmityApp.Shared.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AmityApp.Api.Services;

public class AuthService
{
    private readonly AmityDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly PhotoUploadService _photoUploadService;
    private readonly IConfiguration _configuration;

    public AuthService(AmityDbContext context,
        IPasswordHasher<User> passwordHasher,
        PhotoUploadService photoUploadService,
        IConfiguration configuration)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _photoUploadService = photoUploadService;
        _configuration = configuration;
    }

    public async Task<ApiResult<Guid>> RegisterAsync(RegisterDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
        {
            return ApiResult<Guid>.Failure("User exists");
        }

        try
        {
            var user = new User
            {
                Email = dto.Email,
                Name = dto.Name,
            };

           user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return ApiResult<Guid>.Success(user.Id);
        }

        catch (Exception ex) 
        {
            return ApiResult<Guid>.Failure(ex.Message);
        }
    }

    public async Task<ApiResult> UploadPhotoAsync(Guid userId, IFormFile photo)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user is null)
            return ApiResult.Failure("User does not exist");

        try
        {
            var (photoPath, photoUrl) = await _photoUploadService.SavePhotoAsync(photo, "uploads", "images", "users");
            user.PhotoPath = photoPath;
            user.PhotoUrl = photoUrl;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return ApiResult.Success();
        }
        catch (Exception ex)
        {
            return ApiResult.Failure(ex.Message);
        }
    }

    public async Task<ApiResult<LoginResponseDto>> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user is null)
            return ApiResult<LoginResponseDto>.Failure("User does not exist");

        var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if(passwordVerificationResult != PasswordVerificationResult.Success)
            return ApiResult<LoginResponseDto>.Failure("Invalid credentials. Try again");

        var jwt = GenerateJwtToken(user);
        var loggedInUser = new LoggedInUser(user.Id, user.Name, user.Email, user.PhotoUrl);
        var loginResponse = new LoginResponseDto(loggedInUser, jwt);
        return ApiResult<LoginResponseDto>.Success(loginResponse);
    }

    private string GenerateJwtToken(User user)

    {
        Claim[] claims = [
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("UserPhotoUrl", user.PhotoUrl ?? string.Empty),
            ];

        var secretKey = _configuration.GetValue<string>("Jwt:SecretKey");
        var securityKey = System.Text.Encoding.UTF8.GetBytes(secretKey);
        var symmetricKey = new SymmetricSecurityKey(securityKey);
        var signingCredentials = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256);

        var jwtSecurityToken = new JwtSecurityToken(signingCredentials: signingCredentials,
            issuer: _configuration.GetValue<string>("Jwt:Issuer"),
            expires: DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("Jwt:ExpireInMinutes")),
            claims: claims);

        var jwt = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

        return jwt;
    }
}