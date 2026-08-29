namespace Alohomora.Core.Common.Helpers;

public static class PhoneNumberHelper
{
    public static string NormalizePhoneNumber(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // حذف فاصله‌ها و بقیه کارکتر های غیر عددی
        input = input
            .Replace(" ", "")
            .Replace("(", "")
            .Replace(")", "")
            .Replace("-", "")
            .Replace("_", "")
            .Trim();

        // تبدیل +98 به 0
        if (input.StartsWith("+98"))
            input = "0" + input[3..];

        // تبدیل 0098 به 0
        else if (input.StartsWith("0098"))
            input = "0" + input[4..];

        // اگر 10 رقم بود و 0 نداشت، 0 اضافه شود
        if (input.Length == 10 && !input.StartsWith('0'))
            input = "0" + input;

        return input;
    }

    public static bool IsValidPhoneNumber(string input)
    {
        // بررسی طول و شروع با 09
        if (input.Length != 11 || !input.StartsWith("09"))
            return false;

        // بررسی اینکه فقط عدد باشد
        return input.All(char.IsDigit);
    }
}
