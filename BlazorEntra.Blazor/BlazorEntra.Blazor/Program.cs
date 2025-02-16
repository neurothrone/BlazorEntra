using Microsoft.AspNetCore.Components.Authorization;
using BlazorEntra.Blazor.Components;
using BlazorEntra.Blazor.Services;
using BlazorEntra.Shared.Models;
using BlazorEntra.Shared.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingAuthenticationStateProvider>();

builder.Services.AddHttpClient("FromBlazorServerToWebAPI", config =>
{
    config.BaseAddress = new Uri(
        builder.Configuration["WebAPIBaseAddress"] ??
        throw new Exception("WebAPIBaseAddress is missing.")
    );
});
builder.Services.AddHttpClient("BlazorServer", config =>
{
    config.BaseAddress = new Uri(
        builder.Configuration["BlazorServerAPIBaseAddress"] ??
        throw new Exception("BlazorServerAPIBaseAddress is missing.")
    );
});

builder.Services.AddScoped<ISkillService, ServerSkillService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorEntra.Blazor.Client._Imports).Assembly);

app.MapGet("/blazor/skills", () =>
{
    List<Skill> skills =
    [
        new() { Id = 1, Name = "Sarcasm" },
        new() { Id = 2, Name = "Irony" },
        new() { Id = 3, Name = "Satire" },
        new() { Id = 4, Name = "Parody" },
        new() { Id = 5, Name = "Wit" },
    ];
    return Results.Ok(skills);
});

app.Run();