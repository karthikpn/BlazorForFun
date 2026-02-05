using Chat.Components;
using Chat.Components.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSignalR();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "https://localhost:5173",
            ValidAudience = "https://localhost:5173",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Your-Super-Secret-Key-That-Is-At-Least-32-Chars-Long!!"))
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context => {
                Console.WriteLine("Token failed: " + context.Exception.Message);
                return Task.CompletedTask;
            }
        };
    });

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