using SiteForge.Core.DTOs;

namespace SiteForge.Core.Interfaces.Services;

public interface AssetService
{
    Task<List<AssetDto>> GetBySiteAsync(Guid siteId);
    Task<AssetDto> CreateAsync(UploadAssetRequest request);
}
