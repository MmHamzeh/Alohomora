namespace Alohomora.Domain.Interfaces;


internal interface IVm
{

}

internal interface IViewModel : IVm
{

    internal DateTime CreatedOn { get; set; }
    internal Guid CreatedBy { get; set; }

    internal DateTime? ModifiedOn { get; set; }
    internal Guid? ModifiedBy { get; set; }
}

internal interface IEnmVm<T> : IVm where T : Enum
{
    internal string Title { get; set; }
    internal string TitleEn { get; set; }
    internal string Description { get; set; }
}

internal interface IFileVm : IVm
{
    internal string FileName { get; set; }
    internal string FileExtension { get; set; }
    internal long Size { get; set; }
    internal MimeTypeEnm MimeType { get; set; }

    internal byte[] FileContext { get; set; }
    internal string Description { get; set; }
}
