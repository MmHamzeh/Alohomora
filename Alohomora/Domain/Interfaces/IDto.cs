namespace Alohomora.Domain.Interfaces;

internal abstract class IDto
{
    private StringBuilder? ErrorMessages { get; set; }


    internal virtual bool IsValid()
        => ErrorMessages?.Length == 0;

    internal virtual void PrepareDto(Guid currentUserGuid, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        return;
    }

    protected void AddErrorMessage(string errorMessages)
    {
        ErrorMessages ??= new StringBuilder();
        ErrorMessages.AppendLine(errorMessages);
    }

    internal string GetErrorMessage()
        => ErrorMessages?.ToString() ?? string.Empty;

}
