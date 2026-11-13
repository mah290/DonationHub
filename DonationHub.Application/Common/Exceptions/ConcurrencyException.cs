using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Application.Common.Exceptions
{
    public sealed class ConcurrencyException(Exception? inner = null) : Exception("A concurrency conflict occurred. The entity has been modified by another process.", inner);

}
