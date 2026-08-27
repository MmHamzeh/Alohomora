namespace Alohomora.Domain.Models.DtoModels.ExternalModels;



internal abstract class BaseUser
{
    internal string ApiKey { get; set; }
}

#region BulkSMS
internal class SendBulkSMSDto : BaseUser
{
    internal string Text { get; set; }
    internal long Sender { get; set; }
    internal SendBulkRecipient[] Recipients { get; set; }
}

internal partial class SendBulkRecipient
{
    internal long Destination { get; set; }
    internal long? UserTraceId { get; set; }
}

#endregion

#region MultipleSMS

internal class SendMultipleSMSDto : BaseUser
{
    internal SendMultipleRecipient[] Recipients { get; set; }
}
internal partial class SendMultipleRecipient
{
    internal long Sender { get; set; }
    internal string Text { get; set; }
    internal long Destination { get; set; }
    internal long UserTraceId { get; set; }
}

#endregion

#region MultipleToken

internal class SendMultipleTokenDto : BaseUser
{
    internal SendMultipleTokenRecipient[] Recipients { get; set; }
    internal string TemplateKey { get; set; }
}
internal partial class SendMultipleTokenRecipient
{
    internal long Destination { get; set; }
    internal long UserTraceId { get; set; }
    internal string[] Parameters { get; set; }
}

#endregion

#region GetStatus

internal class GetStatusByIdDto : BaseUser
{
    internal long[] Ids { get; set; }
}
internal class GetStatusByUserTraceIdsDto : BaseUser
{
    internal long[] UserTraceIds { get; set; }
}

#endregion

