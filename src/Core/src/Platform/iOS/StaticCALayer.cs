using System.Diagnostics.CodeAnalysis;
using CoreAnimation;

namespace Microsoft.Maui.Platform;

class StaticCALayer : CALayer, IAutoSizableCALayer
{
	[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Proven safe in MauiCALayerAutosizeToSuperLayerBehavior_DoesNotLeak test.")]
    readonly MauiCALayerAutosizeToSuperLayerBehavior _autosizeToSuperLayerBehavior = new();

	protected override void Dispose(bool disposing)
	{
		_autosizeToSuperLayerBehavior.Detach();
		base.Dispose(disposing);
	}

	public override void RemoveFromSuperLayer()
	{
		_autosizeToSuperLayerBehavior.Detach();
		base.RemoveFromSuperLayer();
	}

	void IAutoSizableCALayer.AutoSizeToSuperLayer()
	{
		_autosizeToSuperLayerBehavior.AttachOrThrow(this);
	}

	public override void AddAnimation(CAAnimation animation, string? key)
	{
		// Do nothing, we don't want animations here
	}
}