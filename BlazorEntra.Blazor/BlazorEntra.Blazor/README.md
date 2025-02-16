# BlazorEntra

### Configure

The contents of `appsettings.Development.json` is the following:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "DetailedErrors": true,
  "Authentication": {
    "Schemes": {
      "EntraIDOpenIDConnect": {
        "Authority": "...",
        "ClientId": "...",
        "ClientSecret": "...",
        "CallbackPath": "/signin-oidc",
        "SignedOutCallbackPath": "/signout-callback-oidc",
        "ResponseType": "code"
      }
    }
  }
}
```