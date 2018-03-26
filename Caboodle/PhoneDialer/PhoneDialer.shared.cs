using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Caboodle
{
    public static partial class PhoneDialer
    {
        internal static void ValidateOpen(string number)
        {
            if (string.IsNullOrWhiteSpace(number))
            {
                throw new ArgumentNullException(nameof(number));
            }

            if (!IsSupported)
            {
                throw new FeatureNotSupportedException();
            }
        }
    }
}
