#if ANDROID
using PlatformView = Android.Views.View;
using PlatformWindow = Android.App.Activity;
#elif IOS || MACCATALYST
using PlatformView = UIKit.UIView;
using PlatformWindow = UIKit.UIWindow;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
using PlatformWindow = Microsoft.UI.Xaml.Window;
#endif

namespace Microsoft.Maui.Controls.Embedding;

/// <summary>
/// This class is just here to serve as a reflection-based bridge to the EmbeddingExtensions class.
/// </summary>
static class EmbeddingReflection
{
	public static MauiAppBuilder UseMauiEmbeddedApp<TApp>(this MauiAppBuilder builder)
		where TApp : class, IApplication
	{
		typeof(DynamicResourceExtension).Assembly // Controls.Xaml
			.GetType("Microsoft.Maui.Controls.Embedding.EmbeddingExtensions")!
			.GetMethod("UseMauiEmbeddedApp", 1, [ typeof(MauiAppBuilder) ])!
			.MakeGenericMethod(typeof(TApp))
			.Invoke(null, [ builder ]);
		return builder;
	}

	public static IMauiContext CreateEmbeddedWindowContext(this MauiApp mauiApp, PlatformWindow platformWindow)
	{
		return (IMauiContext)typeof(Application).Assembly // Controls.Core
			.GetType("Microsoft.Maui.Controls.Embedding.EmbeddingExtensions")!
			.GetMethod("CreateEmbeddedWindowContext", [ typeof(MauiApp), typeof(PlatformWindow) ])!
			.Invoke(null, [ mauiApp, platformWindow ])!;
	}

	public static PlatformView ToPlatformEmbedded(this IElement element, IMauiContext context)
	{
		return (PlatformView)typeof(Application).Assembly // Controls.Core
			.GetType("Microsoft.Maui.Controls.Embedding.EmbeddingExtensions")!
			.GetMethod("ToPlatformEmbedded", [ typeof(IElement), typeof(IMauiContext) ])!
			.Invoke(null, [ element, context ])!;
	}

	public static PlatformView ToPlatformEmbedded(this IElement element, MauiApp mauiApp, PlatformWindow platformWindow)
	{
		return (PlatformView)typeof(Application).Assembly // Controls.Core
			.GetType("Microsoft.Maui.Controls.Embedding.EmbeddingExtensions")!
			.GetMethod("ToPlatformEmbedded", [ typeof(IElement), typeof(MauiApp), typeof(PlatformWindow) ])!
			.Invoke(null, [ element, mauiApp, platformWindow ])!;
	}
}
