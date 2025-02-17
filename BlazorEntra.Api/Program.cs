using BlazorEntra.Shared.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Allow requests from the Blazor WASM host from JavaScript (JS interop is used
// by WASM) 
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWASMOrigin",
        policy =>
        {
            policy.WithOrigins(
                "https://localhost:7002",
                "http://localhost:5002"
            );
        });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://login.microsoftonline.com/6afcb002-8d80-4456-b725-476445b8e55d/v2.0";
        options.Audience = "api://0be1ffd3-cb67-4654-a028-22f93726afe1";
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = JwtRegisteredClaimNames.Name,
            RoleClaimType = "role",
            ValidIssuer = "https://sts.windows.net/6afcb002-8d80-4456-b725-476445b8e55d/"
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowWASMOrigin");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/skills", () =>
{
    List<Skill> skills =
    [
        new() { Id = 1, Name = "C#" },
        new() { Id = 2, Name = "Blazor" },
        new() { Id = 3, Name = "Python" },
        new() { Id = 4, Name = "React" },
        new() { Id = 5, Name = "JavaScript" },
        new() { Id = 6, Name = "TypeScript" },
        new() { Id = 7, Name = "SQL" },
        new() { Id = 8, Name = "HTML" },
        new() { Id = 9, Name = "CSS" },
    ];
    return Results.Ok(skills);
}).RequireAuthorization();

app.Run();