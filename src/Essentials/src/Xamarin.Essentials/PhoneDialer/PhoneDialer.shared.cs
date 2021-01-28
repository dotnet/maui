using System;

namespace Xamarin.Essentials
{
    public static partial class PhoneDialer
    {
        internal static void ValidateOpen(string number)
        {
            if (string.IsNullOrWhiteSpace(number))
                throw new ArgumentNullException(nameof(number));

            if (!IsSupported)
                throw new FeatureNotSupportedException();
        }

        public static void Open(string number)
            => PlatformOpen(number);
    }
}
