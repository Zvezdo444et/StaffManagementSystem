using Inspector.Models;

namespace Inspector.Services;

public interface IPensionAgeSettingsService
{
    Task<PensionAgeSetting?> GetSettingsAsync(CancellationToken ct = default);
    Task SaveSettingsAsync(PensionAgeSetting settings, CancellationToken ct = default);
}
