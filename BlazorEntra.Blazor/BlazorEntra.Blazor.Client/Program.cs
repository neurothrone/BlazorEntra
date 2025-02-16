using BlazorEntra.Blazor.Client;
using BlazorEntra.Blazor.Client.Services;
using BlazorEntra.Shared.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
// builder.Services.AddOptions();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();

builder.Services.AddKeyedScoped<HttpClient>(
    "FromBlazorWasmToWebAPI",
    (_, _) =>
        new HttpClient
        {
            BaseAddress = new Uri(
                builder.Configuration["WebAPIBaseAddress"] ??
                throw new Exception("WebAPIBaseAddress is missing.")
            )
        }
);
builder.Services.AddKeyedScoped<HttpClient>(
    "FromBlazorWasmToBlazorServerAPI",
    (_, _) =>
        new HttpClient
        {
            BaseAddress = new Uri(
                builder.Configuration["BlazorServerAPIBaseAddress"] ??
                throw new Exception("BlazorServerAPIBaseAddress is missing.")
            )
        }
);

builder.Services.AddScoped<ISkillService, WasmSkillService>();

await builder.Build().RunAsync();