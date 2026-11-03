using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Application.Common.Exceptions
{
    public enum ErrorType { Validation, Unauthorized, Forbidden, NotFound, Conflict }

    public sealed class AppException : Exception
    {
        public string Code { get; }
        public ErrorType Type { get; }

        public AppException(string code, string message, ErrorType type) : base(message)
        {
            Code = code;
            Type = type;
        }
    }
}
