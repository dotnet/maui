using System;
using System.Runtime.CompilerServices;
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

		[Fact(DisplayName = "Pending Update Does Not Retain Disconnected Indicator")]
		public async Task PendingUpdateDoesNotRetainDisconnectedIndicator()
		{
			var references = await InvokeOnMainThreadAsync(CreateDisconnectedIndicatorWithPendingUpdate);
			try
			{
				await AssertionExtensions.WaitForGC(references.VirtualView);
			}
			finally
			{
				GC.KeepAlive(references.PlatformView);
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		(WeakReference VirtualView, ProgressBar PlatformView) CreateDisconnectedIndicatorWithPendingUpdate()
		{
			var view = new ActivityIndicatorStub { IsRunning = true };
			var handler = CreateHandler(view);
			var platformView = handler.PlatformView;
			Assert.False(platformView.IsAttachedToWindow);

			((IElementHandler)handler).DisconnectHandler();

			return (new WeakReference(view), platformView);
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
