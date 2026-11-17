using Dcd.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dcd.Web.Controllers;

public sealed class AccountController : Controller
{
    [HttpGet("/account/sign-in")]
    public IActionResult SignIn() => View(new SignInViewModel());

    [HttpPost("/account/sign-in")]
    [ValidateAntiForgeryToken]
    public IActionResult SignIn(SignInViewModel model)
    {
        if (ModelState.IsValid)
            ModelState.AddModelError("", "Sign-in is not available yet. Please try again later.");

        model.Password = "";
        ModelState.SetModelValue(nameof(model.Password), "", "");
        return View(model);
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
