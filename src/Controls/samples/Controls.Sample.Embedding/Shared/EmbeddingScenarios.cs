using Microsoft.Maui.Controls.Embedding;
using Microsoft.Maui.Platform;

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

namespace Maui.Controls.Sample;

class EmbeddingScenarios
{
	public interface IScenario
	{
		(MyMauiContent, PlatformView) Embed(PlatformWindow window);
	}

	public class Scenario1_Basic : IScenario
	{
		public (MyMauiContent, PlatformView) Embed(PlatformWindow window)
		{
			var mauiApp = MauiProgram.CreateMauiApp();
#if ANDROID
			var mauiContext = new MauiContext(mauiApp.Services, window);
#else
			var mauiContext = new MauiContext(mauiApp.Services);
#endif
			var mauiView = new MyMauiContent();
			var nativeView = mauiView.ToPlatform(mauiContext);
			return (mauiView, nativeView);
		}
	}

	public class Scenario2_Scoped : IScenario
	{
		public (MyMauiContent, PlatformView) Embed(PlatformWindow window)
		{
			var mauiApp = MauiProgram.CreateMauiApp();
			var mauiView = new MyMauiContent();
			var nativeView = mauiView.ToPlatformEmbedded(mauiApp, window);
			return (mauiView, nativeView);
		}
	}

	public class Scenario3_Correct : IScenario
	{
		public static class MyEmbeddedMauiApp
		{
			static MauiApp? _shared;

			public static MauiApp Shared => _shared ??= MauiProgram.CreateMauiApp();
		}

		PlatformWindow? _window;
		IMauiContext? _windowContext;

		public IMauiContext WindowContext =>
			_windowContext ??= MyEmbeddedMauiApp.Shared.CreateEmbeddedWindowContext(_window ?? throw new InvalidOperationException());

		public (MyMauiContent, PlatformView) Embed(PlatformWindow window)
		{
			_window ??= window;

			var context = WindowContext;
			var mauiView = new MyMauiContent();
			var nativeView = mauiView.ToPlatformEmbedded(context);
			return (mauiView, nativeView);
		}
	}
}
