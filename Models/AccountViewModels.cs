using System.ComponentModel.DataAnnotations;

namespace Dcd.Web.Models;

public sealed class SignInViewModel
{
    [Required(ErrorMessage = "Enter your email address.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(254, ErrorMessage = "Use an email address under 255 characters.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Enter your password.")]
    [StringLength(128, ErrorMessage = "Use 128 characters or fewer.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";
}

public class AccountNameViewModel
{
    [Required(ErrorMessage = "Enter your first name.")]
    [StringLength(100, ErrorMessage = "Use 100 characters or fewer.")]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = "";

    [Required(ErrorMessage = "Enter your last name.")]
    [StringLength(100, ErrorMessage = "Use 100 characters or fewer.")]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = "";
}

public class AccountDetailsViewModel : AccountNameViewModel
{
    [Required(ErrorMessage = "Enter your email address.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(254, ErrorMessage = "Use an email address under 255 characters.")]
    public string Email { get; set; } = "";
}

public sealed class SignUpViewModel : AccountDetailsViewModel
{
    [Required(ErrorMessage = "Create a password.")]
    [MinLength(8, ErrorMessage = "Use at least 8 characters.")]
    [StringLength(128, ErrorMessage = "Use 128 characters or fewer.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Confirm your password.")]
    [StringLength(128, ErrorMessage = "Use 128 characters or fewer.")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = "";
}

public sealed class ProfileViewModel : AccountDetailsViewModel { }

public sealed class ConfirmEmailViewModel
{
    [Required, EmailAddress, StringLength(254)] public string Email { get; set; } = "";
    [Required, StringLength(4000)] public string Token { get; set; } = "";
}

public sealed class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Enter your email address.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(254, ErrorMessage = "Use an email address under 255 characters.")]
    public string Email { get; set; } = "";
}

public sealed class ResetPasswordViewModel
{
    [Required, StringLength(450)] public string Id { get; set; } = "";
    [Required, StringLength(4000)] public string Token { get; set; } = "";

    [Required(ErrorMessage = "Create a password.")]
    [MinLength(8, ErrorMessage = "Use at least 8 characters.")]
    [StringLength(128, ErrorMessage = "Use 128 characters or fewer.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Confirm your password.")]
    [StringLength(128, ErrorMessage = "Use 128 characters or fewer.")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = "";
}
