using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Caboodle
{
    public partial class Preferences
    {
        public bool ContainsKey(string key) =>
            throw new NotImplementedInReferenceAssemblyException();

        public void Remove(string key) =>
            throw new NotImplementedInReferenceAssemblyException();

        public void Clear() =>
            throw new NotImplementedInReferenceAssemblyException();

        void Set<T>(string key, T value) =>
            throw new NotImplementedInReferenceAssemblyException();

        T Get<T>(string key, T defaultValue) =>
            throw new NotImplementedInReferenceAssemblyException();
    }
}
