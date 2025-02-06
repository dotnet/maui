using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;

namespace Microsoft.Maui.Platform;

[Register("MauiCALayerAutosizeObserver")]
class CALayerAutosizeObserver : NSObject
{
	static readonly NSString _boundsKey = new("bounds");

	readonly WeakReference<CALayer> _layerReference;
	bool _disposed;

	public static CALayerAutosizeObserver Attach(CALayer layer)
	{
		_ = layer ?? throw new ArgumentNullException(nameof(layer));

		var superLayer = layer.SuperLayer ?? throw new InvalidOperationException("SuperLayer should be set before creating CALayerAutosizeObserver");
		var observer = new CALayerAutosizeObserver(layer);
		superLayer.AddObserver(observer, _boundsKey, NSKeyValueObservingOptions.New, observer.Handle);
		layer.Frame = superLayer.Bounds;
		return observer;
	}

	private CALayerAutosizeObserver(CALayer layer)
	{
		_layerReference = new WeakReference<CALayer>(layer);
		IsDirectBinding = false;
	}

	[Preserve(Conditional = true)]
	public override void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
	{
		if (!_disposed && keyPath == _boundsKey && context == Handle && _layerReference.TryGetTarget(out var layer))
		{
			layer.Frame = layer.SuperLayer?.Bounds ?? CGRect.Empty;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			_disposed = true;

			if (_layerReference.TryGetTarget(out var layer))
			{
				layer?.SuperLayer?.RemoveObserver(this, _boundsKey);
			}
		}

		base.Dispose(disposing);
	}
}
