using System.Text.RegularExpressions;

namespace HospitalManagement.App.Helpers;

public static class IdentifierValidator
{
    private static readonly Regex SimpleIdentifierRegex =
        new(@"^[A-Za-z][A-Za-z0-9_$#]{0,127}$", RegexOptions.Compiled);

    private static readonly Regex PasswordRegex =
        new(@"^[A-Za-z0-9_@$#.!?\-]{6,64}$", RegexOptions.Compiled);

    public static string NormalizeSimpleIdentifier(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{fieldName} không được để trống.");

        var normalized = value.Trim().ToUpperInvariant();

        if (!SimpleIdentifierRegex.IsMatch(normalized))
            throw new ArgumentException($"{fieldName} không hợp lệ. Chỉ hỗ trợ định danh Oracle không dấu nháy kép.");

        return normalized;
    }

    public static string NormalizeQualifiedName(string owner, string objectName)
    {
        return $"{NormalizeSimpleIdentifier(owner, "Owner")}.{NormalizeSimpleIdentifier(objectName, "Object name")}";
    }

    public static string NormalizePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Mật khẩu không được để trống.");

        var trimmed = password.Trim();
        if (!PasswordRegex.IsMatch(trimmed))
            throw new ArgumentException("Mật khẩu chỉ nên dùng ký tự chữ, số và _ @ $ # . ! ? - ; độ dài 6-64.");

        return trimmed;
    }

    public static string NormalizePrivilege(string privilege)
    {
        if (string.IsNullOrWhiteSpace(privilege))
            throw new ArgumentException("Privilege không được để trống.");

        return privilege.Trim().ToUpperInvariant().Replace("  ", " ");
    }

    public static List<string> NormalizeColumns(IEnumerable<string> columns)
    {
        return columns
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => NormalizeSimpleIdentifier(x, "Column"))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
