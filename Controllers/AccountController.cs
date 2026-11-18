using Dcd.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Dcd.Web.Controllers;

public sealed class AccountController(IConfiguration configuration) : Controller
{
    [HttpGet("/account/sign-in")]
    public IActionResult SignIn() => View(new SignInViewModel());

    [HttpPost("/account/sign-in")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SignIn(SignInViewModel model, string? returnUrl)
    {
        if (ModelState.IsValid)
        {
            var email = configuration["LocalAdmin:Email"];
            var salt = configuration["LocalAdmin:Salt"];
            var hash = configuration["LocalAdmin:PasswordHash"];
            if (email is not null && salt is not null && hash is not null &&
                string.Equals(model.Email.Trim(), email, StringComparison.OrdinalIgnoreCase) &&
                CryptographicOperations.FixedTimeEquals(Convert.FromBase64String(hash),
                    Rfc2898DeriveBytes.Pbkdf2(model.Password, Convert.FromBase64String(salt), 210000, HashAlgorithmName.SHA256, 32)))
            {
                var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, email),
                    new Claim(ClaimTypes.Role, "Administrator")], "AdminSession");
                await HttpContext.SignInAsync("AdminSession", new ClaimsPrincipal(identity));
                TempData["AdminSignedIn"] = true;
                return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl!) : RedirectToAction("Index", "Admin");
            }
            ModelState.AddModelError("", "The email or password is incorrect.");
        }

        model.Password = "";
        ModelState.SetModelValue(nameof(model.Password), "", "");
        return View(model);
    }

    [HttpPost("/account/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("AdminSession");
        TempData["AdminLoggedOut"] = true;
        return RedirectToAction(nameof(SignIn));
    }

    [HttpGet("/account/sign-up")]
    public IActionResult SignUp() => View(new SignUpViewModel());

    [HttpPost("/account/sign-up")]
    [ValidateAntiForgeryToken]
    public IActionResult SignUp(SignUpViewModel model)
    {
        if (ModelState.IsValid)
            ModelState.AddModelError("", "Account creation is not available yet. Please try again later.");

        model.Password = "";
        model.ConfirmPassword = "";
        ModelState.SetModelValue(nameof(model.Password), "", "");
        ModelState.SetModelValue(nameof(model.ConfirmPassword), "", "");
        return View(model);
    }

    [HttpGet("/account/forgot-password")]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [HttpPost("/account/forgot-password")]
    [ValidateAntiForgeryToken]
    public IActionResult ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        return View("VerifyCode", new RecoveryCodeViewModel());
    }

    [HttpGet("/account/verify-code")]
    public IActionResult VerifyCode() => View(new RecoveryCodeViewModel());

    [HttpPost("/account/verify-code")]
    [ValidateAntiForgeryToken]
    public IActionResult VerifyCode(RecoveryCodeViewModel model)
    {
        // shortcut: recovery is an interface preview, connect account storage and email before enabling verification.
        if (ModelState.IsValid)
            ModelState.AddModelError("", "Email verification is not connected yet. No password has been changed.");

        model.Code = "";
        ModelState.SetModelValue(nameof(model.Code), "", "");
        return View(model);
    }

    [HttpGet("/account/terms")]
    public IActionResult Terms() => View();
}
