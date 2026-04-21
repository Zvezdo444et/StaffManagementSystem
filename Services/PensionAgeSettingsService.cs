using Inspector.Data;
using Inspector.Models;
using Microsoft.EntityFrameworkCore;

namespace Inspector.Services;

public sealed class PensionAgeSettingsService : IPensionAgeSettingsService
{
    private readonly IDbContextFactory<InspectorDbContext> _dbFactory;

    private PensionAgeSetting? _cachedSettings;

    public PensionAgeSettingsService(IDbContextFactory<InspectorDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<PensionAgeSetting?> GetSettingsAsync(CancellationToken ct = default)
    {
        if (_cachedSettings != null)
            return _cachedSettings;

        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        _cachedSettings = await db.PensionAgeSettings
            .OrderBy(p => p.Id)
            .FirstOrDefaultAsync(ct);

        return _cachedSettings;
    }

    public async Task SaveSettingsAsync(PensionAgeSetting settings, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var existing = await db.PensionAgeSettings.FindAsync(new object[] { settings.Id }, ct);
        if (existing != null)
        {
            existing.MenAge = settings.MenAge;
            existing.WomenAge = settings.WomenAge;
        }
        else
        {
            db.PensionAgeSettings.Add(settings);
        }

        await db.SaveChangesAsync(ct);

        _cachedSettings = null;
    }
}