using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace AGDPMS.Shared.Validations;

public static class PasswordValidator
{
    public static bool IsValid(string password) => !string.IsNullOrWhiteSpace(password) && _PASSWORD_REGEX.IsMatch(password);

    private static readonly Regex _PASSWORD_REGEX = new(@"^(?=.*\d).{8,}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    public static string DefaultErrorMessage => "Mật khẩu phải dài ít nhất 8 ký tự và bao gồm ít nhất một chữ số.";
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class PasswordAttribute : ValidationAttribute
{
    public PasswordAttribute() => ErrorMessage = PasswordValidator.DefaultErrorMessage;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string password && PasswordValidator.IsValid(password))
            return ValidationResult.Success;
        return new ValidationResult(ErrorMessage);
    }
}
