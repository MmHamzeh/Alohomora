
namespace Alohomora.Services.Contact.AuthServices;

internal static class EndpointScanner
{
    public static List<ControllerDefenition> GetEndpoints()
    {
       return [];
        //TODO: implement this
    }

}

internal class ControllerDefenition
{
    public AttributeDefenition AttributeDefenition { get; set; }
    public List<ActionDefenition> ActionDefenitions { get; set; }
}

internal class ActionDefenition
{
    public AttributeDefenition AttributeDefenition { get; set; }
    public string Route { get; set; }
}

internal class AttributeDefenition
{
    public AuthenticationType AuthenticationType { get; set; }
    public string[] Roles { get; set; }
}

