using CoreAnimation;

namespace Microsoft.Maui.Platform;

class StaticCAShapeLayer : CAShapeLayer
{
	public override void AddAnimation(CAAnimation animation, string? key)
	{
		// Do nothing, we don't want animations here
	}
}