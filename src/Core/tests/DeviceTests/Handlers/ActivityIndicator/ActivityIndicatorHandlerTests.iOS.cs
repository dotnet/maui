using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ActivityIndicatorHandlerTests
	{
		UIActivityIndicatorView GetNativeActivityIndicator(ActivityIndicatorHandler activityIndicatorHandler) =>
			(UIActivityIndicatorView)activityIndicatorHandler.NativeView;

		bool GetNativeIsRunning(ActivityIndicatorHandler activityIndicatorHandler) =>
			GetNativeActivityIndicator(activityIndicatorHandler).IsAnimating;

		async Task ValidateColor(IActivityIndicator activityIndicator, Color color, Action action = null)
		{
			var expected = await GetValueAsync(activityIndicator, handler =>
			{
				var native = GetNativeActivityIndicator(handler);
				action?.Invoke();
				return native.BackgroundColor.ToColor();
			});
			Assert.Equal(expected, color);
		}
	}
}