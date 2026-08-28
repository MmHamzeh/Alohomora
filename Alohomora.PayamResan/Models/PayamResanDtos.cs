namespace Alohomora.PayamResan.Models;



public abstract class BaseUser
{
    public string ApiKey { get; set; }
}

#region BulkSMS
public class SendBulkSMSDto : BaseUser
{
    public string Text { get; set; }
    public long Sender { get; set; }
    public SendBulkRecipient[] Recipients { get; set; }
}

public partial class SendBulkRecipient
{
    public long Destination { get; set; }
    public long? UserTraceId { get; set; }
}

#endregion

#region MultipleSMS

public class SendMultipleSMSDto : BaseUser
{
    public SendMultipleRecipient[] Recipients { get; set; }
}
public partial class SendMultipleRecipient
{
    public long Sender { get; set; }
    public string Text { get; set; }
    public long Destination { get; set; }
    public long UserTraceId { get; set; }
}

#endregion

#region MultipleToken

public class SendMultipleTokenDto : BaseUser
{
    public SendMultipleTokenRecipient[] Recipients { get; set; }
    public string TemplateKey { get; set; }
}
public partial class SendMultipleTokenRecipient
{
    public long Destination { get; set; }
    public long UserTraceId { get; set; }
    public string[] Parameters { get; set; }
}

#endregion

#region GetStatus

public class GetStatusByIdDto : BaseUser
{
    public long[] Ids { get; set; }
}
public class GetStatusByUserTraceIdsDto : BaseUser
{
    public long[] UserTraceIds { get; set; }
}

#endregion

