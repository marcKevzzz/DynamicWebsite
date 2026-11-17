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

public sealed class SignUpViewModel
{
    [Required(ErrorMessage = "Enter your first name.")]
    [StringLength(100, ErrorMessage = "Use 100 characters or fewer.")]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = "";

    [Required(ErrorMessage = "Enter your last name.")]
    [StringLength(100, ErrorMessage = "Use 100 characters or fewer.")]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = "";

    [Required(ErrorMessage = "Enter your email address.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(254, ErrorMessage = "Use an email address under 255 characters.")]
    public string Email { get; set; } = "";

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

public sealed class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Enter your email address.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(254, ErrorMessage = "Use an email address under 255 characters.")]
    public string Email { get; set; } = "";
}

public sealed class RecoveryCodeViewModel
{
    [Required(ErrorMessage = "Enter all six digits.")]
    [RegularExpression("^[0-9]{6}$", ErrorMessage = "Enter a six-digit code using numbers only.")]
    [Display(Name = "Verification code")]
    public string Code { get; set; } = "";
}
