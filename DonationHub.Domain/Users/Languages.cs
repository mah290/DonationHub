using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Domain.Users
{
    public static class Languages
    {
        public const string English = "en";
        public const string Arabic = "ar";

        public static bool IsSupported(string? code) => code is English or Arabic;
    }
}
