# Vefa.CustomAuth Quickstart

An OAuth2 / OpenID Connect quickstart — in the spirit of the Duende IdentityServer
quickstarts — built entirely on the **`Vefa.CustomAuth`** NuGet package family.

It contains three ASP.NET Core projects:

| Project | Role | Default URL |
| --- | --- | --- |
| **`src/AuthServer`** | Authorization server (OAuth2 / OIDC provider). Razor Pages UI for login/logout. EF Core + SQLite persistence. | `https://localhost:5001` |
| **`src/WebClient`** | Relying-party web app. Signs users in with OpenID Connect and calls the API. Razor Pages. | `https://localhost:5002` |
| **`src/Api`** | Protected resource. Validates JWT access tokens and authorizes by scope. | `https://localhost:5003` |

## Packages used

- `Vefa.CustomAuth.Server` (pulls in `Vefa.CustomAuth.AspNetCore` + `Vefa.CustomAuth.EntityFrameworkCore`)
- `Vefa.CustomAuth.AspNetCore` — relying-party client integration (`AddCustomAuthClient`)
- `Microsoft.AspNetCore.Authentication.JwtBearer` — API token validation

## How it fits together

```
                     1. GET /Secure (not signed in)
  Browser ───────────────────────────────────────────────▶ WebClient (5002)
        ◀── 302 challenge ──────────────────────────────────────┘
        │
        │ 2. /connect/authorize  (code + PKCE)
        ▼
  AuthServer (5001) ── not signed in ──▶ /Account/Login  (Razor Page)
        │  3. POST credentials → SSO session cookie
        │  4. redirect back to /connect/authorize → ?code=...
        ▼
  WebClient /signin-oidc  ── 5. POST /connect/token (code+verifier) ──▶ AuthServer
        ◀── access_token + id_token + refresh_token ───────────────────┘
        │
        │ 6. GET /identity  (Authorization: Bearer access_token)
        ▼
  Api (5003) ── validates issuer + signature (JWKS) + "api1" scope ──▶ 200 OK
```

The authorization-server packages ship **no HTML** — the host owns the login/logout
pages. This quickstart implements them as Razor Pages and opens the SSO session with
`HttpContext.SignInCustomAuthAsync(...)`.

## Running

Requires the .NET 8 runtime (projects target `net8.0`).

Trust the local HTTPS dev certificate once:

```bash
dotnet dev-certs https --trust
```

Then run each project in its own terminal:

```bash
dotnet run --project src/AuthServer    # https://localhost:5001
dotnet run --project src/Api           # https://localhost:5003
dotnet run --project src/WebClient     # https://localhost:5002
```

Open **https://localhost:5002**, click **Sign in**, and log in with a test user.
On the **Secure** page you can inspect your tokens/claims and call the protected API.

### Test users

| Username | Password |
| --- | --- |
| `alice` | `Pass123$` |
| `bob`   | `Pass123$` |

(Seeded automatically on first run, alongside the signing key, scopes, and the
`web-client` registration.)

## Configuration

Everything is driven from `appsettings.json` (overridable via environment variables):

- **AuthServer** — `ConnectionStrings:CustomAuth` (SQLite, default `customauth.db`),
  `CustomAuth:Issuer`, `CustomAuth:RequireHttps`, `Clients:WebClient:BaseUrl`.
- **WebClient** — `CustomAuth:Authority`, `CustomAuth:ApiBaseUrl`, `CustomAuth:RequireHttpsMetadata`.
- **Api** — `CustomAuth:Authority`, `CustomAuth:RequireHttpsMetadata`.

The SQLite database is created automatically on startup (`EnsureCreated`) and is
git-ignored.

## The `web-client` registration

- Public client — authorization code flow with **PKCE**, no client secret.
- Refresh tokens enabled (`offline_access`).
- Redirect URI: `https://localhost:5002/signin-oidc`
- Post-logout redirect URI: `https://localhost:5002/signout-callback-oidc`
- Allowed scopes: `openid profile email offline_access api1`

## Endpoints (AuthServer)

```
GET  /.well-known/openid-configuration
GET  /.well-known/jwks.json
GET  /connect/authorize
POST /connect/token
GET  /connect/userinfo
GET  /connect/logout
POST /connect/revoke
```

> **macOS note:** port `5000` is used by the AirPlay Receiver (Control Center).
> The default (HTTPS) profiles avoid it; the AuthServer `http` fallback profile
> uses `5010`.
