namespace Alohomora.Domain.Enums;

internal enum MimeTypeEnm
{
    [Description("نامشخص")]
    Unknown = 0,

    [Description("متن")]
    Text = 1,

    [Description("تصویر")]
    Image = 2,

    [Description("ویدئو")]
    Video = 3,

    [Description("صوتی")]
    Voice = 4,

    [Description("موسیقی")]
    Music = 5,

    [Description("PDF")]
    Pdf = 6,

    [Description("مایکروسافت_ورد")]
    Word = 7,

    [Description("مایکروسافت_پاورپوینت")]
    PowerPoint = 8,

    [Description("اکسل")]
    Excel = 9,

    [Description("XML")]
    Xml = 10,

    [Description("Json")]
    Json = 11,

    [Description("آرشیو")]
    Archive = 12,

    [Description("اجراشونده")]
    Executable = 13,

    [Description("بانک_داده")]
    Database = 14,

    [Description("سایر")]
    Other = 32767
}