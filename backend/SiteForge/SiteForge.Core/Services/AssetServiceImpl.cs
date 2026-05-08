using SiteForge.Core.DTOs;
using SiteForge.Core.Entities;
using SiteForge.Core.Interfaces.Repositories;
using SiteForge.Core.Interfaces.Services;
using SiteForge.Core.Utilities;

namespace SiteForge.Core.Services;

public class AssetServiceImpl : AssetService
{
    private readonly RAssetRepository _assets;

    public AssetServiceImpl(RAssetRepository assets)
    {
        _assets = assets;
    }

    public async Task<List<AssetDto>> GetBySiteAsync(Guid siteId) =>
        (await _assets.GetBySiteIdAsync(siteId)).Select(Mappers.ToDto).ToList();

    public async Task<AssetDto> CreateAsync(UploadAssetRequest request)
    {
        var asset = await _assets.AddAsync(new Asset
        {
            SiteId = request.SiteId,
            FileName = request.FileName,
            MimeType = request.MimeType,
            FileSize = request.FileSize,
            StoragePath = request.StoragePath,
            PublicUrl = request.PublicUrl,
            Width = request.Width,
            Height = request.Height,
            AltText = request.AltText,
            Source = request.Source
        });
        return Mappers.ToDto(asset);
    }
}
