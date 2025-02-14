using System.Diagnostics.CodeAnalysis;
using CoreAnimation;

namespace Microsoft.Maui.Platform;

class StaticCALayer : CALayer, IAutoSizableCALayer
{
	[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Proven safe in CALayerAutosizeObserver_DoesNotLeak test.")]
	CALayerAutosizeObserver? _boundsObserver;

	protected override void Dispose(bool disposing)
	{
		_boundsObserver?.Dispose();
		_boundsObserver = null;
		base.Dispose(disposing);
	}

	public override void RemoveFromSuperLayer()
	{
		_boundsObserver?.Dispose();
		_boundsObserver = null;
		base.RemoveFromSuperLayer();
	}

	void IAutoSizableCALayer.AutoSizeToSuperLayer()
	{
		_boundsObserver?.Dispose();
		_boundsObserver = CALayerAutosizeObserver.Attach(this);
	}

	public override void AddAnimation(CAAnimation animation, string? key)
	{
		// Do nothing, we don't want animations here
	}
}