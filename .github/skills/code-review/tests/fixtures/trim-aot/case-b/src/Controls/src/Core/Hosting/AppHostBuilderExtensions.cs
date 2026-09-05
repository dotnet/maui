using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls.Hosting;

public static partial class AppHostBuilderExtensions
{
	internal static IMauiHandlersCollection AddControlsHandlers(this IMauiHandlersCollection handlersCollection)
	{
		handlersCollection.AddHandler<Page, PageHandler>();
		handlersCollection.AddHandler<WebView, WebViewHandler>();
		const string hybridWebViewDynamicFeatures = "HybridWebView uses dynamic System.Text.Json serialization features.";
		if (RuntimeFeature.IsHybridWebViewSupported)
		{
			// Keep the RequiresDynamicCode path isolated under the HybridWebView feature switch
			// so NativeAOT/full-trim apps don't pick up HybridWebView warnings just by
			// evaluating the default handler registration list.
			AddHybridWebViewHandler(handlersCollection);
		}
#if !NETSTANDARD
		[RequiresDynamicCode(hybridWebViewDynamicFeatures)]
#endif
		[RequiresUnreferencedCode(hybridWebViewDynamicFeatures)]
		static void AddHybridWebViewHandler(IMauiHandlersCollection handlersCollection)
		{
			handlersCollection.AddHandler<HybridWebView, HybridWebViewHandler>();
		}

		handlersCollection.AddHandler<Border, BorderHandler>();
		return handlersCollection;
	}
}
