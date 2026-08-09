using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Handlers;

[RequiresUnreferencedCode(DynamicFeatures)]
#if !NETSTANDARD
[RequiresDynamicCode(DynamicFeatures)]
#endif
public partial class HybridWebViewHandler : IHybridWebViewHandler
{
	internal const string DynamicFeatures = "HybridWebView uses dynamic System.Text.Json serialization features.";
}
