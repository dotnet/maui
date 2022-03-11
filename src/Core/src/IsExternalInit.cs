#nullable enable

// https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.isexternalinit?view=net-5.0
// https://developercommunity.visualstudio.com/t/error-cs0518-predefined-type-systemruntimecompiler/1244809
// Adding this because at least one of the target frameworks doesn't include it; hopefully we can drop this at some point
// (and hopefully before release)
// TODO ezhart Evaluate whether we still need this
namespace System.Runtime.CompilerServices
{
	internal static class IsExternalInit { }
}
