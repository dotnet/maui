using System;
using CoreAnimation;

namespace Microsoft.Maui.Platform;

class StaticCAShapeLayer : CAShapeLayer, IAutoSizableCALayer
{
	void IAutoSizableCALayer.AutoSizeToSuperLayer()
	{
		Frame = SuperLayer?.Bounds ?? throw new InvalidOperationException("SuperLayer should be set before invoking AutoSizeToSuperLayer");
		this.SetAutoresizeToSuperLayer(true);
	}

	public override void AddAnimation(CAAnimation animation, string? key)
	{
		// Do nothing, we don't want animations here
	}
}