using System;
using System.Diagnostics.CodeAnalysis;
using CoreAnimation;
using CoreGraphics;
using Foundation;

namespace Microsoft.Maui.Platform;

class StaticCALayer : CALayer, IAutoSizableCALayer
{
	[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Sublayer already holds a reference to SuperLayer by design.")]
	IDisposable? _boundsObserver;

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		_boundsObserver?.Dispose();
		_boundsObserver = null;
	}

	public override void RemoveFromSuperLayer()
	{
		_boundsObserver?.Dispose();
		_boundsObserver = null;
		base.RemoveFromSuperLayer();
	}

	public void AutoSizeToSuperLayer()
	{
		var superLayer = SuperLayer ?? throw new InvalidOperationException("SuperLayer should be set before calling AutoSizeToSuperLayer");
		_boundsObserver?.Dispose();
		_boundsObserver = superLayer.AddObserver("bounds", NSKeyValueObservingOptions.New, _ =>
		{
			Frame = SuperLayer?.Bounds ?? CGRect.Empty;
		});

		Frame = superLayer.Bounds;
	}
	
	public override void AddAnimation(CAAnimation animation, string? key)
	{
		// Do nothing, we don't want animations here
	}
}