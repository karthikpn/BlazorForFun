using Chat.Components;
using Chat.Components.Hubs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Text;

public class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();
        builder.Services.AddSignalR();

        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "Auth0";
            })
            .AddCookie()
            .AddOpenIdConnect("Auth0", options =>
             {
                 var auth0 = builder.Configuration.GetSection("Auth0");

                 options.Authority = $"https://{auth0["Domain"]}";
                 options.ClientId = auth0["ClientId"];
                 options.ClientSecret = auth0["ClientSecret"];
                 options.ResponseType = "code";
                 options.CallbackPath = "/account/callback";

                 options.Scope.Clear();
                 options.Scope.Add("openid");
                 options.Scope.Add("profile");
                 options.Scope.Add("email");

                 options.SaveTokens = true;

                 options.Events = new OpenIdConnectEvents
                 {
                     OnRemoteFailure = context =>
                     {
                         // Handle authentication failures (e.g., user cancelled login)
                         var errorMessage = context.Failure?.Message ?? "";
                         
                         // Check if user denied authorization
                         if (errorMessage.Contains("access_denied"))
                         {
                             context.HandleResponse();
                             context.Response.Redirect("/");
                             return Task.CompletedTask;
                         }

                         // For other failures, redirect to home with error info
                         context.HandleResponse();
                         context.Response.Redirect("/?error=auth_failed");
                         return Task.CompletedTask;
                     },
                     OnRedirectToIdentityProviderForSignOut = context =>
                     {
                         var logoutUri = $"https://{auth0["Domain"]}/v2/logout?client_id={auth0["ClientId"]}";

                         var postLogoutUri = context.Properties.RedirectUri;
                         if (!string.IsNullOrEmpty(postLogoutUri))
                         {
                             if (postLogoutUri.StartsWith("/"))
                             {
                                 postLogoutUri = $"{context.Request.Scheme}://{context.Request.Host}{postLogoutUri}";
                             }
                             logoutUri += $"&returnTo={Uri.EscapeDataString(postLogoutUri)}";
                         }

                         context.Response.Redirect(logoutUri);
                         context.HandleResponse();
                         return Task.CompletedTask;
                     }
                 };
             });


        builder.Configuration.AddUserSecrets<Program>();
        builder.Services.AddControllers();
        builder.Services.AddAuthorization();


        var app = builder.Build();
        app.UseAuthentication(); // Must be first
        app.UseAuthorization();
        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        app.UseHttpsRedirection();

        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapControllers();
        app.MapHub<ChatHub>("/chatHub");
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}