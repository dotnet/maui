using System;
using CoreAnimation;
using UIKit;

namespace Microsoft.Maui.Platform;

class StaticCALayer : CALayer, IAutoSizableCALayer
{
	void IAutoSizableCALayer.AutoSizeToSuperLayer()
	{
		this.SetMauiAutoSizeToSuperLayer(true);
	}

	public override void AddAnimation(CAAnimation animation, string? key)
	{
		// Do nothing, we don't want animations here
	}
}