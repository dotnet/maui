using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;
using AProgressBar = Android.Widget.ProgressBar;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ProgressBarHandlerTests
	{
		AProgressBar GetNativeProgressBar(ProgressBarHandler progressBarHandler) =>
			progressBarHandler.PlatformView;

		double GetNativeProgress(ProgressBarHandler progressBarHandler) =>
			(double)GetNativeProgressBar(progressBarHandler).Progress / ProgressBarExtensions.Maximum;

		Task ValidateNativeProgressColor(IProgress progressBar, Color color, Action action = null) =>
			 ValidateHasColor(progressBar, color, action);

		[Fact(DisplayName = "Null Progress Color Resets To Default")]
		public async Task NullProgressColorResetsToDefault()
		{
			var progressBar = new ProgressBarStub()
			{
				Progress = 0.9,
				ProgressColor = Colors.Red,
			};

			var tintCleared = await GetValueAsync(progressBar, handler =>
			{
				var native = GetNativeProgressBar(handler);

				// The color was applied through the tint list.
				if (native.ProgressTintList is null)
					return false;

				progressBar.ProgressColor = null;

				return native.ProgressTintList is null;
			});

			Assert.True(tintCleared);
		}
	}
}