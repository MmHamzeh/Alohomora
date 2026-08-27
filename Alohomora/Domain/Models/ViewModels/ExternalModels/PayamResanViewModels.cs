namespace Alohomora.Domain.Models.ViewModels.ExternalModels;

internal class SMSOutputGenericModel<T>
{
    internal bool Success { get; set; }

    internal int? ErrorCode { get; set; }

    internal string Error { get; set; }

    internal T Result { get; set; }
}

internal class SendSMSOutput
{
    internal long Id { get; set; }

    internal long? UserTraceId { get; set; }
}

internal class SendTokenOutput
{
    internal long Id { get; set; }
    internal long? UserTraceId { get; set; }

    internal long Sender { get; set; }

    internal string FinalText { get; set; }

}
internal class TokenListOutput
{
    internal string Key { get; set; }
    internal string TextTemplate { get; set; }
    internal string VoiceTemplate { get; set; }
    internal long Status { get; set; }
}
internal class SMSStatusOutput
{
    internal long Id { get; set; }
    internal long? UserTraceId { get; set; }
    internal long StatusCode { get; set; }
    internal string Status { get; set; }
}
internal class SMSAcountIfoOutput
{
    internal long Credit { get; set; }
    internal long[] AvailableSenders { get; set; }
}
