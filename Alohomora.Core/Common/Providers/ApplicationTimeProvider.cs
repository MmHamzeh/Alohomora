using Alohomora.Core.Common.Configs;

namespace Alohomora.Core.Common.Providers;

public class ApplicationTimeProvider(TimeProvider timeProvider)
{
    private readonly bool _useUtcTime = ApplicationConfig.UseUtcTime;

    public DateTime GetDateTime() => _useUtcTime ? timeProvider.GetUtcNow().DateTime : timeProvider.GetLocalNow().DateTime;
    public DateTimeOffset GetDateTimeOffset() => _useUtcTime ? timeProvider.GetUtcNow() : timeProvider.GetLocalNow();

}
