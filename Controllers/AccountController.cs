using Dcd.Web.Data;
using Dcd.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Dcd.Web.Controllers;

[AutoValidateAntiforgeryToken]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class AccountController(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> sessions,
    AppDbContext db, IConfiguration configuration, ILogger<AccountController> logger) : Controller
{
    [AllowAnonymous, HttpGet("/account/sign-in")]
    public IActionResult SignIn() => View(new SignInViewModel());

    [AllowAnonymous, HttpPost("/account/sign-in")]
    public async Task<IActionResult> SignIn(SignInViewModel model, string? returnUrl)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await users.FindByEmailAsync(model.Email.Trim());
        if (user is { Active: true } &&
            (await sessions.PasswordSignInAsync(user, model.Password, isPersistent: false, lockoutOnFailure: true)).Succeeded)
        {
            user.LastSignInAt = DateTime.UtcNow;
            await users.UpdateAsync(user);
            if (Url.IsLocalUrl(returnUrl)) return LocalRedirect(returnUrl!);
            if (await users.IsInRoleAsync(user, "Administrator"))
            {
                TempData["AdminSignedIn"] = true;
                return RedirectToAction("Index", "Admin");
            }
            return RedirectToAction("Index", "Home");
        }
        ModelState.AddModelError("", "The email or password is incorrect.");
        return View(model);
    }

    [Authorize, HttpPost("/account/logout")]
    public async Task<IActionResult> Logout()
    {
        await sessions.SignOutAsync();
        TempData["AdminLoggedOut"] = true;
        return RedirectToAction(nameof(SignIn));
    }

    [AllowAnonymous, HttpGet("/account/sign-up")]
    public IActionResult SignUp() => View(new SignUpViewModel());

    [AllowAnonymous, HttpPost("/account/sign-up")]
    public async Task<IActionResult> SignUp(SignUpViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var email = model.Email.Trim();
        var user = new ApplicationUser
        {
            FirstName = model.FirstName.Trim(), LastName = model.LastName.Trim(),
            Email = email, UserName = email, Active = true, JoinedAt = DateTime.UtcNow
        };
        await using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var created = await users.CreateAsync(user, model.Password);
            if (!created.Succeeded)
            {
                AddErrors(created);
                return View(model);
            }
            await LogActivityAsync(user, "User registered");
            await transaction.CommitAsync();
        }
        catch (DbUpdateException ex) when (IsDuplicate(ex))
        {
            ModelState.AddModelError("", "An account already uses that email address.");
            return View(model);
        }
        if (RecoveryMail.FromConfiguration(configuration) is not null) await SendConfirmationAsync(user, email);
        TempData["AccountNotice"] = "Account created. Sign in to view the portfolio.";
        return RedirectToAction(nameof(SignIn));
    }

    private static ProfileViewModel ProfileFor(ApplicationUser user) => new()
    {
        FirstName = user.FirstName, LastName = user.LastName, Email = user.Email!
    };

    [Authorize, HttpGet("/account/profile")]
    public async Task<IActionResult> Profile()
    {
        var user = await users.GetUserAsync(User);
        return user is not { Active: true } ? Challenge() : View(ProfileFor(user));
    }

    [Authorize, HttpPost("/account/profile")]
    public async Task<IActionResult> Profile(AccountNameViewModel model)
    {
        var user = await users.GetUserAsync(User);
        if (user is not { Active: true }) return Challenge();
        bool dialog = Request.GetTypedHeaders().Accept?.Any(type => type.MediaType.Value == "application/json") == true;
        IActionResult InvalidProfile() => dialog
            ? UnprocessableEntity(new { errors = ModelState.Values.SelectMany(value => value.Errors).Select(error => error.ErrorMessage) })
            : View(ProfileFor(user));
        if (!ModelState.IsValid) return InvalidProfile();
        user.FirstName = model.FirstName.Trim();
        user.LastName = model.LastName.Trim();
        await using var transaction = await db.Database.BeginTransactionAsync();
        var updated = await users.UpdateAsync(user);
        if (!updated.Succeeded)
        {
            AddErrors(updated);
            return InvalidProfile();
        }
        await LogActivityAsync(user, "Profile updated");
        await transaction.CommitAsync();
        if (dialog) return Json(new { firstName = user.FirstName, lastName = user.LastName });
        TempData["AccountNotice"] = "Your profile has been saved.";
        return RedirectToAction(nameof(Profile));
    }

    private Task LogActivityAsync(ApplicationUser user, string action) =>
        ActivityLog.RecordAsync(db, action, $"{user.FirstName} {user.LastName}", "users", user.Email!);

    private static bool IsDuplicate(DbUpdateException ex) => ex.InnerException is SqlException { Number: 2601 or 2627 };
    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
    }

    private async Task<bool> SendConfirmationAsync(ApplicationUser user, string email)
    {
        var token = await users.GenerateEmailConfirmationTokenAsync(user);
        return await SendMail(email, Url.Action(nameof(ConfirmEmail), new { email, token })!, "Confirm your dcd. email");
    }

    private async Task<bool> SendMail(string email, string path, string subject)
    {
        var mail = RecoveryMail.FromConfiguration(configuration);
        if (mail is null) return false;
        try { await mail.SendAsync(email, path, subject, "Open this link to continue.", HttpContext.RequestAborted); return true; }
        catch (System.Net.Mail.SmtpException ex) { logger.LogWarning("Mail delivery failed with status {Status}", ex.StatusCode); return false; }
    }

    [Authorize, HttpGet("/account/confirm-email")]
    public IActionResult ConfirmEmail(ConfirmEmailViewModel model) => ModelState.IsValid ? View(model) : BadRequest();

    [Authorize, HttpPost("/account/confirm-email"), ActionName(nameof(ConfirmEmail))]
    public async Task<IActionResult> ConfirmAddress([FromForm] ConfirmEmailViewModel model)
    {
        if (!ModelState.IsValid) return View("ConfirmEmail", model);
        var user = await users.GetUserAsync(User);
        if (user is null) return Challenge();
        if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase)) return BadRequest();
        var result = await users.ConfirmEmailAsync(user, model.Token);
        if (!result.Succeeded) { AddErrors(result); return View("ConfirmEmail", model); }
        return RedirectToAction(nameof(Profile));
    }

    [AllowAnonymous, HttpGet("/account/forgot-password")]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [AllowAnonymous, HttpPost("/account/forgot-password"), EnableRateLimiting("recovery")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        if (RecoveryMail.FromConfiguration(configuration) is null)
        { ModelState.AddModelError("", "Email recovery is unavailable."); return View(model); }
        var user = await users.FindByEmailAsync(model.Email.Trim());
        if (user is { Active: true, EmailConfirmed: true, Email: not null })
        {
            var token = await users.GeneratePasswordResetTokenAsync(user);
            await SendMail(user.Email, Url.Action(nameof(ResetPassword), new { id = user.Id, token })!, "Reset your dcd. password");
        }
        else if (user is { Active: true, Email: not null }) await SendConfirmationAsync(user, user.Email);
        TempData["AccountNotice"] = "If an active account uses that address, check its inbox.";
        return RedirectToAction(nameof(SignIn));
    }

    [HttpGet("/account/verify-code")]
    public IActionResult VerifyCode() => RedirectToAction(nameof(ForgotPassword));

    [HttpGet("/account/reset-password")]
    public IActionResult ResetPassword(string id, string token) => ModelState.IsValid ? View(new ResetPasswordViewModel { Id = id, Token = token }) : BadRequest();

    [HttpPost("/account/reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await users.FindByIdAsync(model.Id);
        if (user is { Active: true, EmailConfirmed: true })
        {
            var result = await users.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded) return RedirectToAction(nameof(SignIn));
            AddErrors(result);
        }
        else ModelState.AddModelError("", "The reset link is invalid or expired.");
        return View(model);
    }

    [HttpGet("/account/terms")]
    public IActionResult Terms() => View();

}
