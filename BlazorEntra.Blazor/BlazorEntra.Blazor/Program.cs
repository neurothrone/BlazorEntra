using Microsoft.AspNetCore.Components.Authorization;
using BlazorEntra.Blazor.Components;
using BlazorEntra.Blazor.Services;
using BlazorEntra.Shared.Models;
using BlazorEntra.Shared.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

const string entraIdScheme = "EntraIdOpenIdConnect";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = entraIdScheme;
}).AddOpenIdConnect(entraIdScheme, options =>
{
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    // Navigate to the url in OpenID Connect metadata document link and copy the value of issuer.
    // options.Authority = "...";
    // options.ResponseType = OpenIdConnectResponseType.Code;
    // options.UsePkce = true; // Set to true by default is using ResponseType.Code
    options.Scope.Add(OpenIdConnectScope.OpenIdProfile);
    // options.CallbackPath = new PathString("/signin-oidc");
    // options.SignedOutCallbackPath = new PathString("/signout-callback-oidc");
    // options.ClientId = "...";
    // options.ClientSecret = "...";
    options.MapInboundClaims = false;
    options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
    options.TokenValidationParameters.RoleClaimType = "role";
    options.SaveTokens = true;
}).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

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

app.MapGet("/login", (string? returnUrl, HttpContext httpContext) =>
{
    // Ensure the returnUrl is valid & safe.  
    returnUrl = ValidateUri(httpContext, returnUrl);
    return TypedResults.Challenge(new AuthenticationProperties
    {
        RedirectUri = returnUrl
    });
}).AllowAnonymous();

app.MapPost("/logout", ([FromForm] string? returnUrl, HttpContext httpContext) =>
{
    returnUrl = ValidateUri(httpContext, returnUrl);
    return TypedResults.SignOut(
        new AuthenticationProperties { RedirectUri = returnUrl },
        [
            CookieAuthenticationDefaults.AuthenticationScheme,
            entraIdScheme
        ]
    );
});

app.Run();

public partial class Program
{
    private static string ValidateUri(HttpContext httpContext, string? uri)
    {
        string basePath = string.IsNullOrEmpty(httpContext.Request.PathBase)
            ? "/"
            : httpContext.Request.PathBase;

        if (string.IsNullOrEmpty(uri))
        {
            return basePath;
        }
        else if (!Uri.IsWellFormedUriString(uri, UriKind.Relative))
        {
            return new Uri(uri, UriKind.Absolute).PathAndQuery;
        }
        else if (uri[0] != '/')
        {
            return $"{basePath}{uri}";
        }

        return uri;
    }
}