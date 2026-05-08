using SqlSugar;
using SiteForge.Core.Entities;
using SiteForge.Core.Interfaces.Repositories;

namespace SiteForge.Infrastructure.Repositories;

public class AssetRepository : BaseRepository<Asset>, RAssetRepository
{
    public AssetRepository(ISqlSugarClient db) : base(db) { }

    public Task<List<Asset>> GetBySiteIdAsync(Guid siteId) =>
        _db.Queryable<Asset>().Where(x => x.SiteId == siteId && !x.IsDeleted).OrderByDescending(x => x.CreatedAt).ToListAsync();

    public Task<List<Asset>> GetBySiteAndTypeAsync(Guid siteId, string mimeType) =>
        _db.Queryable<Asset>().Where(x => x.SiteId == siteId && x.MimeType == mimeType && !x.IsDeleted).OrderByDescending(x => x.CreatedAt).ToListAsync();
}
