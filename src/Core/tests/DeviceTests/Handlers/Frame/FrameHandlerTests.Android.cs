using System;
using System.Threading.Tasks;
using AndroidX.CardView.Widget;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests
{
	public partial class FrameHandlerTests
	{
		CardView GetNativeFrame(FrameHandler frameHandler) =>
			frameHandler.NativeView;

		Task ValidateBackgroundColor(IFrame frame, Color color, Action action = null) =>
			ValidateHasColor(frame, color, action);

		Task ValidateBorderColor(IFrame frame, Color color, Action action = null) =>
			ValidateHasColor(frame, color, action);

		Task ValidateHasColor(IFrame frame, Color color, Action action = null)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeFrame = GetNativeFrame(CreateHandler(frame));
				action?.Invoke();
				nativeFrame.AssertContainsColor(color);
			});
		}

		bool HasNativeShadow(FrameHandler frameHandler)
		{
			var nativeFrame = GetNativeFrame(frameHandler);
			return nativeFrame.CardElevation != 0;
		}
	}
}