using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using ObjCRuntime;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ActivityIndicatorHandlerTests
	{
		UIActivityIndicatorView GetNativeActivityIndicator(ActivityIndicatorHandler activityIndicatorHandler) =>
			activityIndicatorHandler.PlatformView;

		bool GetNativeIsRunning(ActivityIndicatorHandler activityIndicatorHandler) =>
			GetNativeActivityIndicator(activityIndicatorHandler).IsAnimating;

		[Fact(DisplayName = "Setting IsRunning False While Visible Should Hide Native View")]
		public async Task SettingIsRunningFalseWhileVisibleShouldHideNativeView()
		{
			var activityIndicator = new ActivityIndicatorStub
			{
				IsRunning = true,
				Visibility = Visibility.Visible
			};

			bool isAnimating = true;
			bool isHidden = false;

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler(activityIndicator);

				activityIndicator.IsRunning = false;
				handler.UpdateValue(nameof(IActivityIndicator.IsRunning));

				var nativeView = GetNativeActivityIndicator(handler);
				isAnimating = nativeView.IsAnimating;
				isHidden = nativeView.Hidden;
			});

			Assert.False(isAnimating, "ActivityIndicator should stop animating when IsRunning is false.");
			Assert.True(isHidden, "ActivityIndicator should be hidden when IsRunning is false.");
		}

	}
}