using DonationHub.Application.Features.Campaigns;
using DonationHub.Domain.Campaigns;
using Microsoft.EntityFrameworkCore;

namespace DonationHub.Infrastructure.Persistence;

internal sealed class CampaignRepository(AppDbContext context)
    : GenericRepository<Campaign>(context), ICampaignRepository
{
    
    public override Task<Campaign?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Context.Campaigns.FirstOrDefaultAsync(c => c.Id == id, ct);

    public void SetExpectedRowVersion(Campaign campaign, byte[] rowVersion) =>
        Context.Entry(campaign).Property(c => c.RowVersion).OriginalValue = rowVersion;
}