using GuradzoApi;
using GuradzoApi.Models;
using GuradzoApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<GuradoDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// JWT config
var key = builder.Configuration["Jwt:Key"];
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TokenService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();


// Register endpoint
app.MapPost("/api/register", async (LoginRequest request, UserService userService) =>
{
    if (await userService.UserExistsAsync(request.UserName))
        return Results.BadRequest("User already exists");

    try
    {
        await userService.CreateUserAsync(request.UserName, request.Password);
        return Results.Ok("User registered successfully");
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

// Reset password endpoint
app.MapPost("/api/reset-password", async (ResetPasswordRequest request, UserService userService) =>
{
    var result = await userService.ResetPasswordAsync(request.Username, request.NewPassword);
    return result
        ? Results.Ok("Password updated successfully")
        : Results.NotFound("User not found");
});


app.MapPost("/api/login", async (LoginRequest request, TokenService tokenService, UserService userService) =>
{
    if (await userService.ValidateUserAsync(request.UserName, request.Password))
    {
        var accessToken = tokenService.GenerateAccessToken(request.UserName);
        var refreshToken = tokenService.GenerateRefreshToken();
        await tokenService.SaveRefreshTokenAsync(request.UserName, refreshToken);

        return Results.Ok(new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        });
    }

    return Results.Unauthorized();
});

app.MapPost("/api/refresh", async (LoginRequest request, TokenService tokenService) =>
{
    if (await tokenService.ValidateRefreshTokenAsync(request.UserName, request.Password))
    {
        var newAccessToken = tokenService.GenerateAccessToken(request.UserName);
        var newRefreshToken = tokenService.GenerateRefreshToken();

        await tokenService.SaveRefreshTokenAsync(request.UserName, newRefreshToken);

        return Results.Ok(new LoginResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        });
    }

    return Results.Unauthorized();
});


app.MapControllers();

app.Run();
