namespace Alohomora.Core.Domain.Interfaces;

public interface IDbEnm<T> : IDbTable<T> where T : Enum
{
    internal string Title { get; set; }
    internal string TitleEn { get; set; }
    internal string Description { get; set; }
}
