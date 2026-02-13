using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Chat.Components.Controllers
{
    [Route("account")]
    public class LoginController : Controller
    {
        [HttpGet("login")]
        public IActionResult Login(string returnUrl = "/")
        {
            return Challenge(new AuthenticationProperties
            {
                RedirectUri = returnUrl
            }, "Auth0");
        }

        [HttpGet("oidc-logout")]
        public async Task<IActionResult> Logout()
        {
            var authProperties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("", "/", null, Request.Scheme)
            };

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync("Auth0", authProperties);

            return new EmptyResult();
        }

        [HttpGet("logout")]
        public IActionResult LogoutCallback()
        {
            return Redirect("/logout-callback");
        }
    }
}

