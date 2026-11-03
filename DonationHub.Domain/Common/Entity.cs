using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Domain.Common
{
    public abstract class Entity
    {
        public Guid Id { get; protected set; }

        protected Entity(Guid id) => Id = id;

        protected Entity() { } // for EF Core
    }
}
