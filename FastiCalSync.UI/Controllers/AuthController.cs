using FastiCalSync.UI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace FastiCalSync.UI.Controllers
{
    [Route("")]
    public class AuthController : Controller
    {
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger logger;

        public AuthController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<HomeController> logger)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.logger = logger;
        }

        [TempData]
        public string ErrorMessage { get; set; }

        [HttpGet("sign-in")]
        public IActionResult SignIn(string returnUrl = null)
        {
            var redirectUrl = Url.Action("SignInCallback", null, new { returnUrl });
            var properties = signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
            return Challenge(properties, "Google");
        }

        [HttpGet("sign-in-callback")]
        public async Task<IActionResult> SignInCallback(
            string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToAction("Index");
            }

            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null) return RedirectToAction("Index", "Home");

            // Sign in the user with this external login provider if the user already has a login.
            var signInResult = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey,
                isPersistent: false, bypassTwoFactor: true);
            if (signInResult.IsLockedOut) throw new NotImplementedException("Expected account to be unlocked.");

            // This is the user's first sign in, create an account for them.
            if (!signInResult.Succeeded)
            {
                ApplicationUser user = new ApplicationUser();
                user.From(info);
                var createUserResult = await userManager.CreateAsync(user);
                if (!createUserResult.Succeeded)
                    throw new NotImplementedException("Expected user creation to succeed.");

                var addLoginResult = await userManager.AddLoginAsync(user, info);
                if (!addLoginResult.Succeeded)
                    throw new NotImplementedException("Expected login addition to succeed.");

                await signInManager.SignInAsync(user, isPersistent: false);
                logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
            }
            else
            {
                ApplicationUser user = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                user.From(info);
                await userManager.UpdateAsync(user);
            }

            logger.LogInformation("User logged in with {Name} provider.", info.LoginProvider);
            return Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl)
                : (IActionResult)RedirectToAction("Index", "Home");
        }

        [HttpPost("sign-out"), ValidateAntiForgeryToken]
        public async Task<IActionResult> SignOut()
        {
            await signInManager.SignOutAsync();
            logger.LogInformation("User logged out.");
            return RedirectToAction("Index", "Home");
        }
    }
}
