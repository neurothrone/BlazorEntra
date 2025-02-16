using System.Security.Claims;
using BlazorEntra.Blazor.Client;
using BlazorEntra.Blazor.Client.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorEntra.Blazor.Services;

// This is a server-side AuthenticationStateProvider that uses PersistentComponentState to flow the
// authentication state to the client which is then fixed for the lifetime of the WebAssembly application.
internal sealed class PersistingAuthenticationStateProvider : AuthenticationStateProvider,
    IHostEnvironmentAuthenticationStateProvider, IDisposable
{
    private readonly PersistentComponentState persistentComponentState;
    private readonly PersistingComponentStateSubscription subscription;
    private Task<AuthenticationState>? authenticationStateTask;

    public PersistingAuthenticationStateProvider(PersistentComponentState state)
    {
        persistentComponentState = state;
        subscription = state.RegisterOnPersisting(OnPersistingAsync, RenderMode.InteractiveWebAssembly);
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync() => authenticationStateTask ??
                                                                               throw new InvalidOperationException(
                                                                                   $"Do not call {nameof(GetAuthenticationStateAsync)} outside of the DI scope for a Razor component. Typically, this means you can call it only within a Razor component or inside another DI service that is resolved for a Razor component.");

    public void SetAuthenticationState(Task<AuthenticationState> task)
    {
        authenticationStateTask = task;
    }

    private async Task OnPersistingAsync()
    {
        var authenticationState = await GetAuthenticationStateAsync();
        var principal = authenticationState.User;

        if (principal.Identity?.IsAuthenticated == true)
        {
            var userId = GetRequiredClaim(principal, "sub");
            var name = GetRequiredClaim(principal, "name");

            if (userId != null && name != null)
            {
                persistentComponentState.PersistAsJson(nameof(UserInfo), new UserInfo
                {
                    UserId = userId,
                    Name = name,
                });
            }
        }
    }

    private string GetRequiredClaim(ClaimsPrincipal principal, string claimType) =>
        principal.FindFirst(claimType)?.Value ??
        throw new InvalidOperationException($"Could not find required '{claimType}' claim.");

    public void Dispose()
    {
        subscription.Dispose();
    }
}