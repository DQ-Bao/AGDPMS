using System.ComponentModel.DataAnnotations;

namespace AGDPMS.Shared.Validations;

public static class BirthDayValidator
{
    public static bool IsValid(DateTime birthDay)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDay.Year;
        if (birthDay > today.AddYears(-age)) age--;
        return age >= 0;
    }

    public static string DefaultErrorMessage => "Ngày sinh không hợp lệ.";
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class BirthDayAttribute : ValidationAttribute
{
    public BirthDayAttribute() => ErrorMessage = BirthDayValidator.DefaultErrorMessage;
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateTime birthDay && BirthDayValidator.IsValid(birthDay))
            return ValidationResult.Success;
        return new ValidationResult(ErrorMessage);
    }
}
