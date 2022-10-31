using System;
using System.Threading.Tasks;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ActivityIndicatorHandlerTests
	{
		ProgressBar GetNativeActivityIndicator(ActivityIndicatorHandler activityIndicatorHandler) =>
			activityIndicatorHandler.PlatformView;

		bool GetNativeIsRunning(ActivityIndicatorHandler activityIndicatorHandler) =>
			GetNativeActivityIndicator(activityIndicatorHandler).Visibility == ViewStates.Visible;

		Task ValidateHasColor(IActivityIndicator activityIndicator, Color color, Action action = null)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeActivityIndicator = GetNativeActivityIndicator(CreateHandler(activityIndicator));
				action?.Invoke();
				nativeActivityIndicator.AssertContainsColorAsync(color);
			});
		}

		[Theory(DisplayName = "Visibility is set correctly")]
		[InlineData(Visibility.Visible)]
		[InlineData(Visibility.Collapsed)]
		[InlineData(Visibility.Hidden)]
		public override async Task SetVisibility(Visibility visibility)
		{
			var view = new ActivityIndicatorStub
			{
				Visibility = visibility,
				IsRunning = true
			};

			var id = await GetValueAsync(view, handler => GetVisibility(handler));
			Assert.Equal(view.Visibility, id);
		}
	}
}