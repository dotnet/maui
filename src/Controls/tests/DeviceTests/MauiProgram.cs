using System.Collections.Generic;
using System.Reflection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.TestUtils.DeviceTests.Runners;

namespace Microsoft.Maui.DeviceTests
{
	public static class MauiProgram
	{
#if ANDROID
		public static Android.Content.Context CurrentContext => MauiProgramDefaults.DefaultContext;
#elif WINDOWS
		public static Microsoft.UI.Xaml.Window CurrentWindow => MauiProgramDefaults.DefaultWindow;
#endif

		public static IApplication DefaultTestApp => MauiProgramDefaults.DefaultTestApp;

		public static MauiApp CreateMauiApp()
		{
#if IOS || MACCATALYST

			// https://github.com/dotnet/maui/issues/11853
			// I'd like to just have this added to the tests this relates to but 
			// due to the issue above, I have to do it here for now. 
			// Once 11853 has been resolved, I'll move this back into the relevant test files.
			Controls.Element
				.ControlsElementMapper
				.ModifyMapping(AutomationProperties.IsInAccessibleTreeProperty.PropertyName, (handler, view, action) =>
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