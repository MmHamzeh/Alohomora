using Alohomora.Core.Common.Enums;

namespace Alohomora.Core.Services.Contact.AuthServices;

internal static class AlohomoraPolicy
{
    private const string Prefix = "Alohomora";

    internal static string BuildPolicyName(AuthenticationType type, IEnumerable<string>? roles)
    {
        if (type == AuthenticationType.CheckRoles)
            return $"{Prefix}.{type}.{string.Join(",", roles ?? [])}";

        return $"{Prefix}.{type}";
    }

    internal static bool TryParse(string policyName, out AuthenticationType type, out string[]? roles)
    {
        type = default;
        roles = null;

        if (policyName.StartsWith(Prefix + ".", StringComparison.Ordinal) is false)
            return false;

        var parts = policyName.Split('.', 3);
        if (parts.Length < 2 || Enum.TryParse(parts[1], out type) is false)
            return false;

        if (type == AuthenticationType.CheckRoles)
            roles = parts.Length == 3 ? parts[2].Split(',', StringSplitOptions.RemoveEmptyEntries) : [];

        return true;
    }
}
