namespace Alohomora.Core.Domain.Models.ViewModels.ExternalModels;

public class SMSOutputGenericModel<T>
{
    internal bool Success { get; set; }

    internal int? ErrorCode { get; set; }

    internal string Error { get; set; }

    internal T Result { get; set; }
}

public class SendSMSOutput
{
    internal long Id { get; set; }

    internal long? UserTraceId { get; set; }
}

public class SendTokenOutput
{
    internal long Id { get; set; }
    internal long? UserTraceId { get; set; }

    internal long Sender { get; set; }

    internal string FinalText { get; set; }

}
public class TokenListOutput
{
    internal string Key { get; set; }
    internal string TextTemplate { get; set; }
    internal string VoiceTemplate { get; set; }
    internal long Status { get; set; }
}
public class SMSStatusOutput
{
    internal long Id { get; set; }
    internal long? UserTraceId { get; set; }
    internal long StatusCode { get; set; }
    internal string Status { get; set; }
}
public class SMSAcountIfoOutput
{
    internal long Credit { get; set; }
    internal long[] AvailableSenders { get; set; }
}
