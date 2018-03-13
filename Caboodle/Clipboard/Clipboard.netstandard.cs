using System.Threading.Tasks;

namespace Microsoft.Caboodle
{
    public static partial class Clipboard
    {
        public static Task SetTextAsync(string text) =>
            throw new NotImplentedInReferenceAssembly();

        public static bool HasText =>
            throw new NotImplentedInReferenceAssembly();

        public static Task<string> GetTextAsync() =>
            throw new NotImplentedInReferenceAssembly();
    }
}
