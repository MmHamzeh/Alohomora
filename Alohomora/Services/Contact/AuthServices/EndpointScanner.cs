
using Alohomora.Core.Common.Enums;

namespace Alohomora.Core.Services.Contact.AuthServices;

internal static class EndpointScanner
{
    public static List<ControllerDefenition> GetEndpoints()
    {
       return [];
        //TODO: implement this
    }

}

public class ControllerDefenition
{
    public AttributeDefenition AttributeDefenition { get; set; }
    public List<ActionDefenition> ActionDefenitions { get; set; }
}

public class ActionDefenition
{
    public AttributeDefenition AttributeDefenition { get; set; }
    public string Route { get; set; }
}

public class AttributeDefenition
{
    public AuthenticationType AuthenticationType { get; set; }
    public string[] Roles { get; set; }
}

