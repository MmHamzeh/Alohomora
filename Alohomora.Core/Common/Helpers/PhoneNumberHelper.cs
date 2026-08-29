namespace Alohomora.Core.Common.Helpers;

public static class PhoneNumberHelper
{
    public static string NormalizePhoneNumber(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // حذف فاصله‌ها و بقیه کارکتر های غیر عددی احتمالی
        //0938 611 4201
        //(0938) 611 4201
        input = input
            .Replace(" ", "")
            .Replace("(", "")
            .Replace(")", "")
            .Trim();

        // تبدیل +98 به 0
        //+989386114201
        if (input.StartsWith("+98"))
            input = "0" + input[3..];

        // تبدیل 98 به 0
        //989386114201
        else if (input.StartsWith("98") && input.Length == 12)
            input = "0" + input[2..];

        // تبدیل 0098 به 0
        //00989386114201
        else if (input.StartsWith("0098"))
            input = "0" + input[4..];

        // اگر 10 رقم بود و 0 نداشت، 0 اضافه شود
        //9386114201
        else if (input.Length == 10 && input.StartsWith('9'))
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
