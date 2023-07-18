using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ActivityIndicatorHandlerTests
	{
		ProgressRing GetNativeActivityIndicator(ActivityIndicatorHandler activityIndicatorHandler) =>
			activityIndicatorHandler.PlatformView;

		bool GetNativeIsRunning(ActivityIndicatorHandler activityIndicatorHandler) =>
			GetNativeActivityIndicator(activityIndicatorHandler).IsActive;

		[Fact(Skip = "Failing on Windows")]
		public override Task SettingSemanticDescriptionMakesElementAccessible()
		{
			return base.SettingSemanticDescriptionMakesElementAccessible();
		}

		[Fact(Skip = "Failing on Windows")]
		public override Task SettingSemanticHintMakesElementAccessible()
		{
			return base.SettingSemanticHintMakesElementAccessible();
		}
	}
}