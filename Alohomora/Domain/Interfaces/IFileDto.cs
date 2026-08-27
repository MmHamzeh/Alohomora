namespace Alohomora.Domain.Interfaces;

internal abstract class IFileDto : IDto
{
    internal string FileName { get; set; }
    internal string FileExtension { get; set; }
    internal long Size { get; set; }
    internal MimeTypeEnm MimeType { get; set; }

    internal byte[] FileContext { get; set; }
    internal string Description { get; set; }
}
