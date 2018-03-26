using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Caboodle
{
    public static partial class Connectivity
    {
        public static NetworkAccess NetworkAccess =>
            throw new NotImplementedInReferenceAssemblyException();

        public static IEnumerable<ConnectionProfile> Profiles =>
            throw new NotImplementedInReferenceAssemblyException();

        static void StartListeners() =>
            throw new NotImplementedInReferenceAssemblyException();

        static void StopListeners() =>
            throw new NotImplementedInReferenceAssemblyException();
    }
}
