using SiteForge.Core.DTOs;
using SiteForge.Core.Entities;
using SiteForge.Core.Interfaces.Repositories;
using SiteForge.Core.Interfaces.Services;
using SiteForge.Core.Utilities;

namespace SiteForge.Core.Services;

public class DomainServiceImpl : DomainService
{
    private readonly RSiteDomainRepository _domains;

    public DomainServiceImpl(RSiteDomainRepository domains)
    {
        _domains = domains;
    }

    public async Task<List<SiteDomainDto>> GetBySiteAsync(Guid siteId) =>
        (await _domains.GetBySiteIdAsync(siteId)).Select(Mappers.ToDto).ToList();

    public async Task<SiteDomainDto> AddAsync(Guid siteId, AddDomainRequest request)
    {
        if (await _domains.DomainExistsAsync(request.Domain))
        {
            throw new InvalidOperationException("Domain already exists.");
        }

        var domain = await _domains.AddAsync(new SiteDomain
        {
            SiteId = siteId,
            Domain = request.Domain.Trim().ToLowerInvariant(),
            IsPrimary = request.IsPrimary,
            VerificationToken = Guid.NewGuid().ToString("N"),
            DnsStatus = "pending"
        });
        return Mappers.ToDto(domain);
    }

    public Task<bool> DeleteAsync(Guid id) => _domains.DeleteAsync(id);

    public async Task<SiteDomainDto?> VerifyAsync(Guid id)
    {
        var domain = await _domains.GetByIdAsync(id);
        if (domain is null) return null;

        domain.IsVerified = true;
        domain.DnsStatus = "configured";
        await _domains.UpdateAsync(domain);
        return Mappers.ToDto(domain);
    }
}
