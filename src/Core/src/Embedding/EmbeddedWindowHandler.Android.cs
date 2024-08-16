using Android.App;

namespace Microsoft.Maui.Embedding;

internal partial class EmbeddedWindowHandler
{
	protected override void ConnectHandler(Activity platformView)
	{
		base.ConnectHandler(platformView);

		UpdateVirtualViewFrame(platformView);
	}

	protected override void DisconnectHandler(Activity platformView)
	{
		base.DisconnectHandler(platformView);
	}

	void UpdateVirtualViewFrame(Activity activity)
	{
		var frame = activity.GetWindowFrame();
		VirtualView.FrameChanged(frame);
	}
}
