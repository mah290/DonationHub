using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Domain.Common
{
    public interface ISoftDeletable
    {
        bool IsDeleted { get; set; }
        DateTime? DeletedAtUtc { get; set; }
    }
}
