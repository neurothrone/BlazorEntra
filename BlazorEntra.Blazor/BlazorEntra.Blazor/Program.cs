using Microsoft.AspNetCore.Components.Authorization;
using BlazorEntra.Blazor.Components;
using BlazorEntra.Blazor.HelperEvents;
using BlazorEntra.Blazor.Services;
using BlazorEntra.Shared.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

const string entraIdScheme = "EntraIDOpenIDConnect";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingAuthenticationStateProvider>();

builder.Services.AddScoped<ISkillService, ServerSkillService>();
builder.Services.AddScoped<SkillRepository>();

// Required by Duende.AccessTokenManagement.OpenIdConnect.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddOpenIdConnectAccessTokenManagement()
    .AddBlazorServerAccessTokenManagement<CustomServerSideTokenStore>();
builder.Services.AddTransient<CustomTokenStorageOidcEvents>();

builder.Services.AddHttpClient("FromBlazorServerToWebAPI", config =>
{
    config.BaseAddress = new Uri(
        builder.Configuration["WebAPIBaseAddress"] ??
        throw new Exception("WebAPIBaseAddress is missing.")
    );
}).AddUserAccessTokenHandler(); // Required by Duende.AccessTokenManagement.OpenIdConnect.

// builder.Services.AddHttpClient("BlazorServer", config =>
// {
//     config.BaseAddress = new Uri(
//         builder.Configuration["BlazorServerAPIBaseAddress"] ??
//         throw new Exception("BlazorServerAPIBaseAddress is missing.")
//     );
// });

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
    options.Scope.Add("api://0be1ffd3-cb67-4654-a028-22f93726afe1/FullAccess");
    options.Scope.Add("offline_access");
    // options.CallbackPath = new PathString("/signin-oidc");
    // options.SignedOutCallbackPath = new PathString("/signout-callback-oidc");
    // options.ClientId = "...";
    // options.ClientSecret = "...";
    options.MapInboundClaims = false;
    options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
    options.TokenValidationParameters.RoleClaimType = "role";
    options.SaveTokens = true;
    options.EventsType = typeof(CustomTokenStorageOidcEvents);
}).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorEntra.Blazor.Client._Imports).Assembly);

app.MapGet("/blazor/skills", (SkillRepository repo) =>
    Results.Ok(repo.GetSkills())
).RequireAuthorization();

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

app.MapGet("/forward-to-web-api/skills", async (ISkillService skillService) =>
    Results.Ok(await skillService.GetSkillsFromApiAsync())
).RequireAuthorization();

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