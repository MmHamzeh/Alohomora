namespace Alohomora.Core.Common.Helpers;

public static class EnumHelper
{
    public static string GetDescription(this Enum @enum)
    {
        try
        {
            var type = @enum.GetType();
            var memberInfos = type.GetMember(@enum.ToString());
            var attributes = memberInfos[0].GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
            var description = ((System.ComponentModel.DescriptionAttribute)attributes[0]).Description;
            return description;
        }
        catch
        {
            return @enum.ToString();
        }
    }
}
