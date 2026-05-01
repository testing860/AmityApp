using AmityApp.Api.Data;
using AmityApp.Api.Data.Entities;
using AmityApp.Api.Endpoints;
using AmityApp.Api.Hubs;
using AmityApp.Api.Services;
using AmityApp.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SocialMediaMaui.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AmityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddTransient<AuthService>()
                .AddTransient<CordialService>()
                .AddTransient<IPasswordHasher<User>, PasswordHasher<User>>()
                .AddTransient<UserService>()
                .AddTransient<PhotoUploadService>()
                .AddTransient<ConnectionService>();


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    var issuer = builder.Configuration.GetValue<string>("Jwt:Issuer");
    var secretKey = builder.Configuration.GetValue<string>("Jwt:SecretKey");
    var securityKey = System.Text.Encoding.UTF8.GetBytes(secretKey);
    var symmetricKey = new SymmetricSecurityKey(securityKey);

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = symmetricKey,
        ValidateAudience = false
    };

    // Development Issuer for Android Emulator
    if (builder.Environment.IsDevelopment())
    {
        options.TokenValidationParameters.ValidIssuers = new[]
        {
            issuer,
            "https://10.0.2.2:7134"
        };
    }
    else
    {
        options.TokenValidationParameters.ValidIssuer = issuer;
    }
});

builder.Services.AddAuthorization();
builder.Services.AddSignalR();

var app = builder.Build();

#if DEBUG
AutoMigrateDb(app.Services);
#endif


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


// Alow Https Redirection for Development
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseAuthentication()
   .UseAuthorization();


app.MapAuthEndpoints()
    .MapCordialsEndpoints()
    .MapUserEndpoints();

app.MapConnectionsEndpoints();

app.MapGet("/ping", () =>
{
    var html = """
    <!DOCTYPE html>
    <html>
    <head><style>body{display:flex;align-items:center;justify-content:center;min-height:100vh;margin:0;background:#1e1e2e;font-family:system-ui,sans-serif;flex-direction:column}h1{font-size:6rem;color:#a6e3a1;margin:0}h2{font-size:3rem;color:#6c7086;margin:0;font-weight:400}</style></head>
    <body><h1>pong!</h1><h2>Amity API is running.</h2></body>
    </html>
    """;
    return Results.Content(html, "text/html");
});

app.MapHub<NotificationsHub>(AppConstants.HubPattern);
app.Run();

static void AutoMigrateDb(IServiceProvider sp)
{
    var scope = sp.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AmityDbContext>();
    if (context.Database.GetPendingMigrations().Any())
        {
        context.Database.Migrate();
        }
}