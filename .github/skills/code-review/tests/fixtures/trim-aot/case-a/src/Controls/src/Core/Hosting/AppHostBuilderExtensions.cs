namespace Microsoft.Maui.Controls.Hosting;

public static partial class AppHostBuilderExtensions
{
	internal static IMauiHandlersCollection AddControlsHandlers(this IMauiHandlersCollection handlersCollection)
	{
		handlersCollection.AddHandler<Page, PageHandler>();
		handlersCollection.AddHandler<WebView, WebViewHandler>();
		if (RuntimeFeature.IsHybridWebViewSupported)
		{
			// NOTE: not registered under NativeAOT or TrimMode=Full scenarios.
			// IL2026/IL3050 are suppressed because the RuntimeFeature.IsHybridWebViewSupported guard
			// has [FeatureGuard(RequiresUnreferencedCodeAttribute)] and [FeatureGuard(RequiresDynamicCodeAttribute)]
			// annotations that should suppress these warnings. The Android NativeAOT ILC does not honor
			// [FeatureGuard] for warning suppression (unlike the iOS/macCatalyst ILC), so we suppress explicitly.
#pragma warning disable IL2026, IL3050
			handlersCollection.AddHandler<HybridWebView, HybridWebViewHandler>();
#pragma warning restore IL2026, IL3050
		}

		handlersCollection.AddHandler<Border, BorderHandler>();
		return handlersCollection;
	}
}
