using CoreAnimation;

namespace Microsoft.Maui.Platform;

class MauiCAClipLayer : CAShapeLayer
{
	public override void AddAnimation(CAAnimation animation, string? key)
	{
		// Do nothing, we don't want animations here
	}
}