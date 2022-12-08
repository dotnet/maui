using System.Collections.Generic;
using System.Reflection;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.TestUtils.DeviceTests.Runners;

namespace Microsoft.Maui.DeviceTests
{
	public static class MauiProgram
	{
#if ANDROID
		public static Android.Content.Context DefaultContext => MauiProgramDefaults.DefaultContext;
#elif WINDOWS
		public static UI.Xaml.Window DefaultWindow => MauiProgramDefaults.DefaultWindow;
#endif

		public static IApplication DefaultTestApp { get; private set; }

		public static MauiApp CreateMauiApp()
		{

#if IOS || MACCATALYST

			// https://github.com/dotnet/maui/issues/11853
			// I'd like to just have this added to the tests this relates to but 
			// due to the issue above, I have to do it here for now. 
			// Once 11853 has been resolved, I'll move this back into the relevant test files.
			ViewHandler
				.ViewMapper
				.ModifyMapping(nameof(IView.Semantics), (handler, view, action) =>
				{
					(handler.PlatformView as UIKit.UIView)?.SetupAccessibilityExpectationIfVoiceOverIsOff();
					action.Invoke(handler, view);
				});
#endif
			return MauiProgramDefaults.CreateMauiApp(new List<Assembly>()
			{
				typeof(MauiProgram).Assembly
			});
		}
	}
}