using System.ComponentModel.DataAnnotations;
using System.Reflection;
using SportEcommerce.Application.Auth;

namespace SportEcommerce.Tests.Auth;

public sealed class RegisterRequestValidationTests
{
    // MVC validates the attributes on the record's constructor parameters, so the test reads them from there.
    private static bool IsValid(string password)
    {
        var parameter = typeof(RegisterRequest).GetConstructors()[0].GetParameters().Single(p => p.Name == "Password");
        var rules = parameter.GetCustomAttributes<ValidationAttribute>();
        return Validator.TryValidateValue(password, new ValidationContext(password), [], rules);
    }

    [Theory]
    [InlineData("Passw0rd!")]
    [InlineData("abcdefg1")]
    public void Accepts_PasswordsWithLetterAndNumber(string password) => Assert.True(IsValid(password));

    [Theory]
    [InlineData("short1")]
    [InlineData("onlyletters")]
    [InlineData("12345678")]
    public void Rejects_WeakPasswords(string password) => Assert.False(IsValid(password));
}
