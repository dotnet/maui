using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Caboodle
{
    class Utils
    {
        public static Version ParseVersion(string version)
        {
            if (Version.TryParse(version, out var number))
                return number;

            return new Version(0, 0);
        }
    }
}
