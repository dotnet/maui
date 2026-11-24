using System;
using System.Threading.Tasks;
using Android.Graphics.Drawables;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;
using AProgressBar = Android.Widget.ProgressBar;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ProgressBarHandlerTests
	{
		[Fact(DisplayName = "Null Progress Color Resets To Default")]
		public async Task NullProgressColorResetsToDefault()
		{
			var progressBar = new ProgressBarStub()
			{
				Progress = 0.5,
				ProgressColor = Colors.Red
			};

			var (filterWhenRed, filterAfterReset, tintAfterReset) = await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler(progressBar);
				var platformProgressBar = GetNativeProgressBar(handler);
				var progressLayer = (platformProgressBar.ProgressDrawable as LayerDrawable)?
					.FindDrawableByLayerId(Android.Resource.Id.Progress) ?? platformProgressBar.ProgressDrawable;
				var red = progressLayer?.ColorFilter;

				progressBar.ProgressColor = null;
				handler.UpdateValue(nameof(IProgress.ProgressColor));

				return (red, progressLayer?.ColorFilter, platformProgressBar.ProgressTintList);
			});

			Assert.NotNull(filterWhenRed);
			// No filter and no tint override left, so the theme's progress color shows again.
			Assert.Null(filterAfterReset);
			Assert.Null(tintAfterReset);
		}

		AProgressBar GetNativeProgressBar(ProgressBarHandler progressBarHandler) =>
			progressBarHandler.PlatformView;

		double GetNativeProgress(ProgressBarHandler progressBarHandler) =>
			(double)GetNativeProgressBar(progressBarHandler).Progress / ProgressBarExtensions.Maximum;

		Task ValidateNativeProgressColor(IProgress progressBar, Color color, Action action = null) =>
			 ValidateHasColor(progressBar, color, action);
	}
}