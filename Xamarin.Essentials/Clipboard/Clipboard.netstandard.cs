using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Clipboard
    {
        public static void SetText(string text)
            => throw new NotImplementedInReferenceAssemblyException();

        public static bool HasText
            => throw new NotImplementedInReferenceAssemblyException();

        public static Task<string> GetTextAsync()
            => throw new NotImplementedInReferenceAssemblyException();
    }
}
