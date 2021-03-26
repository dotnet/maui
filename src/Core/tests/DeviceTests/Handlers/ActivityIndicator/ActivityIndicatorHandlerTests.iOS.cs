using System;
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ActivityIndicatorHandlerTests
	{
		UIActivityIndicatorView GetNativeActivityIndicator(ActivityIndicatorHandler activityIndicatorHandler) =>
			(UIActivityIndicatorView)activityIndicatorHandler.View;

		bool GetNativeIsRunning(ActivityIndicatorHandler activityIndicatorHandler) =>
			GetNativeActivityIndicator(activityIndicatorHandler).IsAnimating;

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