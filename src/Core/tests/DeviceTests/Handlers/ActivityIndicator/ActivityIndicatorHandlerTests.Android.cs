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

		[Fact(DisplayName = "Deferred Show Does Not Resurrect A Hidden Indicator")]
		public async Task DeferredShowDoesNotResurrectHiddenIndicator()
		{
			var activityIndicator = new ActivityIndicatorStub
			{
				IsRunning = true,
				Visibility = Visibility.Visible
			};

			var visibility = await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler(activityIndicator);
				var progressBar = handler.PlatformView;

				activityIndicator.IsRunning = false;
				activityIndicator.Visibility = Visibility.Collapsed;

				ViewStates result = ViewStates.Visible;
				await progressBar.AttachAndRun(async () =>
				{
					await Task.Delay(100);
					result = progressBar.Visibility;
				});

				return result;
			});

			Assert.Equal(ViewStates.Gone, visibility);
		}
	}
}
