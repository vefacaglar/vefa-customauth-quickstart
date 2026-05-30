using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var authority = builder.Configuration["CustomAuth:Authority"] ?? "https://localhost:5001";

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Validate access tokens issued by the Vefa CustomAuth authorization server.
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authority;
        options.RequireHttpsMetadata = builder.Configuration.GetValue("CustomAuth:RequireHttpsMetadata", true);
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = authority,
            // The access token's "aud" is the client id; this API authorizes by
            // scope instead, so audience validation is disabled.
            ValidateAudience = false,
            NameClaimType = "sub",
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Require the "api1" scope (the "scope" claim is a space-delimited string).
    options.AddPolicy("ApiScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
        {
            var scope = context.User.FindFirst("scope")?.Value;
            return scope is not null
                && scope.Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains("api1");
        });
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Protected endpoint: echoes the caller's identity from the access token.
app.MapGet("/identity", (ClaimsPrincipal user) => Results.Ok(new
{
    message = "Hello from the protected API!",
    subject = user.FindFirst("sub")?.Value,
    scopes = user.FindFirst("scope")?.Value,
    claims = user.Claims.Select(c => new { c.Type, c.Value }),
}))
.RequireAuthorization("ApiScope");

app.Run();
