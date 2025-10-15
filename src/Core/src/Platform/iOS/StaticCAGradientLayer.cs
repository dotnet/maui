using CoreAnimation;

namespace Microsoft.Maui.Platform;

class StaticCAGradientLayer : CAGradientLayer, IAutoSizableCALayer
{
	void IAutoSizableCALayer.AutoSizeToSuperLayer()
	{
		this.SetAutoresizeToSuperLayer(true);
	}

	public override void AddAnimation(CAAnimation animation, string? key)
	{
		// Do nothing, we don't want animations here
	}
}