#nullable enable

// Some of the target frameworks (e.g. netstandard2.0) do not include this type, which is
// required by the compiler to support `init` accessors and record types.
namespace System.Runtime.CompilerServices
{
	internal static class IsExternalInit { }
}
