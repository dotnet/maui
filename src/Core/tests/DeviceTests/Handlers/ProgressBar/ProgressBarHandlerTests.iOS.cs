using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using ObjCRuntime;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ProgressBarHandlerTests
	{
		UIProgressView GetNativeProgressBar(ProgressBarHandler progressBarHandler) =>
			progressBarHandler.PlatformView;

		double GetNativeProgress(ProgressBarHandler progressBarHandler) =>
			GetNativeProgressBar(progressBarHandler).Progress;

		[Theory(DisplayName = "Native ProgressBar Bounding Box Honors Explicit Size")]
		[InlineData(1)]
		[InlineData(100)]
		[InlineData(1000)]
		public async Task NativeProgressBarBoundingBoxHonorsExplicitSize(int size)
		{
			var progressBar = new ProgressBarStub
			{
				Height = size,
				Width = size,
				Progress = 0.5,
			};

			var nativeBoundingBox = await GetValueAsync(progressBar, handler => GetNativeProgressBar(handler).GetBoundingBox());

			AssertWithinTolerance(new Size(size, size), nativeBoundingBox.Size);
		}

		async Task ValidateNativeProgressColor(IProgress progressBar, Color color, Action action = null)
		{
			var expected = await GetValueAsync(progressBar, handler =>
			{
				var native = GetNativeProgressBar(handler);
				action?.Invoke();
				return native.ProgressTintColor.ToColor();
			});
			Assert.Equal(expected, color);
		}
	}
}