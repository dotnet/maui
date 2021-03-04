using System;
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using AProgressBar = Android.Widget.ProgressBar;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ProgressBarHandlerTests
	{
		AProgressBar GetNativeProgressBar(ProgressBarHandler progressBarHandler) =>
			(AProgressBar)progressBarHandler.View;

		double GetNativeProgress(ProgressBarHandler progressBarHandler) =>
			(double)GetNativeProgressBar(progressBarHandler).Progress / ProgressBar.Maximum;

		Task ValidateNativeProgressColor(IProgress progressBar, Color color, Action action = null) =>
		   ValidateHasColor(progressBar, color, action);

		Task ValidateHasColor(IProgress progressBar, Color color, Action action = null)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeProgressBar = GetNativeProgressBar(CreateHandler(progressBar));
				action?.Invoke();
				nativeProgressBar.AssertContainsColor(color);
			});
		}
	}
}