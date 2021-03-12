using Microsoft.Maui.Handlers;
using AProgressBar = Android.Widget.ProgressBar;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ProgressBarHandlerTests
	{
		AProgressBar GetNativeProgressBar(ProgressBarHandler progressBarHandler) =>
			(AProgressBar)progressBarHandler.View;

		double GetNativeProgress(ProgressBarHandler progressBarHandler) =>
			(double)GetNativeProgressBar(progressBarHandler).Progress / ProgressBarExtensions.Maximum;
	}
}