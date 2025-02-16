using BlazorEntra.Shared.Models;

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
                )
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowWASMOrigin");
app.UseHttpsRedirection();

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
});

app.Run();