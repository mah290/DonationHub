namespace DonationHub.Domain.Common;

public abstract class AuditableEntity : Entity
{
    public DateTime CreatedAt { get; protected set; }
    public DateTime UpdatedAt { get; protected set; }

    protected AuditableEntity() { }   // for EF Core

    protected void MarkCreated(DateTime now)
    {
        CreatedAt = now;
        UpdatedAt = now;
    }

    protected void Touch(DateTime now) => UpdatedAt = now;
}
