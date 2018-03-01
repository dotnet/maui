using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Caboodle
{
    public partial class Preferences
    {
        public bool ContainsKey(string key) =>
            throw new NotImplentedInReferenceAssemblyException();

        public void Remove(string key) =>
            throw new NotImplentedInReferenceAssemblyException();

        public void Clear() =>
            throw new NotImplentedInReferenceAssemblyException();

        void Set<T>(string key, T value) =>
            throw new NotImplentedInReferenceAssemblyException();

        T Get<T>(string key, T defaultValue) =>
            throw new NotImplentedInReferenceAssemblyException();
    }
}
