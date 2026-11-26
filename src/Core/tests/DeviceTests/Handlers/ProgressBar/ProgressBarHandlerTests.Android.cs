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

			var values = await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler(progressBar);
				var platformProgressBar = GetNativeProgressBar(handler);
				var layers = Assert.IsAssignableFrom<LayerDrawable>(platformProgressBar.ProgressDrawable);
				var progressLayer = layers.FindDrawableByLayerId(global::Android.Resource.Id.Progress);
				var trackLayer = layers.FindDrawableByLayerId(global::Android.Resource.Id.Background);

				var progressFilterWhenRed = progressLayer?.ColorFilter;
				var trackFilterWhenRed = trackLayer?.ColorFilter;

				progressBar.ProgressColor = null;
				handler.UpdateValue(nameof(IProgress.ProgressColor));

				return (
					ProgressFilterWhenRed: progressFilterWhenRed,
					TrackFilterWhenRed: trackFilterWhenRed,
					ProgressFilterAfterReset: progressLayer?.ColorFilter,
					TrackFilterAfterReset: trackLayer?.ColorFilter,
					TintAfterReset: platformProgressBar.ProgressTintList);
			});

			Assert.NotNull(values.ProgressFilterWhenRed);
			Assert.Null(values.TrackFilterWhenRed);
			Assert.Null(values.ProgressFilterAfterReset);
			Assert.Null(values.TrackFilterAfterReset);
			Assert.Null(values.TintAfterReset);
		}

		[Fact(DisplayName = "Null Progress Color Restores The Theme Color")]
		public async Task NullProgressColorRestoresThemeColor()
		{
			var progressBar = new ProgressBarStub()
			{
				Progress = 0.9,
				ProgressColor = Colors.Red
			};

			var themeColor = Color.FromInt(MauiContext.Context.GetThemeAttrColor(global::Android.Resource.Attribute.ColorControlActivated));

			await ValidateHasColor(progressBar, themeColor, () =>
			{
				progressBar.ProgressColor = null;
				progressBar.Handler?.UpdateValue(nameof(IProgress.ProgressColor));
			});
		}

		[Fact(DisplayName = "Progress Color Uses The Configured Tint Mode")]
		public async Task ProgressColorUsesConfiguredTintMode()
		{
			var progressBar = new ProgressBarStub()
			{
				Progress = 0.5
			};

			var filter = await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler(progressBar);
				var platformProgressBar = GetNativeProgressBar(handler);
				platformProgressBar.ProgressTintMode = global::Android.Graphics.PorterDuff.Mode.Multiply;

				progressBar.ProgressColor = Colors.Red;
				handler.UpdateValue(nameof(IProgress.ProgressColor));

				var layers = Assert.IsAssignableFrom<LayerDrawable>(platformProgressBar.ProgressDrawable);
				return layers.FindDrawableByLayerId(global::Android.Resource.Id.Progress)?.ColorFilter;
			});

			Assert.Equal(new global::Android.Graphics.PorterDuffColorFilter(Colors.Red.ToPlatform(), global::Android.Graphics.PorterDuff.Mode.Multiply), filter);
		}

		AProgressBar GetNativeProgressBar(ProgressBarHandler progressBarHandler) =>
			progressBarHandler.PlatformView;

		double GetNativeProgress(ProgressBarHandler progressBarHandler) =>
			(double)GetNativeProgressBar(progressBarHandler).Progress / ProgressBarExtensions.Maximum;

		Task ValidateNativeProgressColor(IProgress progressBar, Color color, Action action = null) =>
			 ValidateHasColor(progressBar, color, action);
	}
}