using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Domain.Common
{
    public sealed class DomainException : Exception
    {
        public string Code { get; }

        public DomainException(string code, string message) : base(message) => Code = code;
    }
}
