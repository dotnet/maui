using System;
using System.Threading.Tasks;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ActivityIndicatorHandlerTests
	{
		ProgressBar GetNativeActivityIndicator(ActivityIndicatorHandler activityIndicatorHandler) =>
			(ProgressBar)activityIndicatorHandler.View;

		bool GetNativeIsRunning(ActivityIndicatorHandler activityIndicatorHandler) =>
			GetNativeActivityIndicator(activityIndicatorHandler).Visibility == ViewStates.Visible;

		Task ValidateColor(IActivityIndicator activityIndicator, Color color, Action action = null) =>
			ValidateHasColor(activityIndicator, color, action);

		Task ValidateHasColor(IActivityIndicator activityIndicator, Color color, Action action = null)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeActivityIndicator = GetNativeActivityIndicator(CreateHandler(activityIndicator));
				action?.Invoke();
				nativeActivityIndicator.AssertContainsColor(color);
			});
		}
	}
}