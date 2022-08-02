using DotnetAuth;
using DotnetAuth.Models;
using DotnetAuth.Repositories;
using DotnetAuth.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

var key = Encoding.ASCII.GetBytes(Settings.Secret);
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}
).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddAuthorization(options =>
{
    foreach (var val in Enum.GetValues(typeof(RoleEnum)))
        options.AddPolicy(val.ToString(), policy => policy.RequireRole(val.ToString()));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.MapPost("login", ([FromBody] UserPostModel model) =>
{
    var user = UserRepository.Get(model.Username, model.Password);

    if (user == null)
        return Results.NotFound(new { message = "Usuário ou senha inválidos !" });

    var token = TokenService.GenerateToken(user);

    return Results.Ok( new
    {
        user = new { username = user.Username, role = user.Role.ToString() },
        token = token
    });
})
.WithName("User");

app.MapGet("anonymous", () =>
{
    return Results.Ok($"Acesso anônimo");
});

app.MapGet("authenticated", [Authorize](ClaimsPrincipal user) =>
{
    return Results.Ok($"Acesso autenticado - {user.Identity.Name}");
}).RequireAuthorization();

app.MapGet("manager", [Authorize] (ClaimsPrincipal user) =>
{
    return Results.Ok($"Acesso autenticado Role:(manager) - {user.Identity.Name}");
}).RequireAuthorization(RoleEnum.Manager.ToString().ToLower());

app.MapGet("employee", [Authorize] (ClaimsPrincipal user) =>
{
    return Results.Ok($"Acesso autenticado Role:(employee) - {user.Identity.Name}");
}).RequireAuthorization(RoleEnum.Employee.ToString().ToLower());

app.Run();

record UserPostModel(string Username, string Password);