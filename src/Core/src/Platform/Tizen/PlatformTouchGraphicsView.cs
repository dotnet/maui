using System;
using System.Collections.Generic;
using System.Text;
using ElmSharp;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Skia.Views;

namespace Microsoft.Maui.Platform
{
	public class PlatformTouchGraphicsView : SkiaGraphicsView
	{
		IGraphicsView? _graphicsView;
		GestureLayer? _gestureLayer;

		public PlatformTouchGraphicsView(EvasObject? parent, IDrawable? drawable = null) : base(parent, drawable)
		{
			_ = parent ?? throw new ArgumentNullException(nameof(parent));
		}

		public void Connect(IGraphicsView graphicsView)
		{
			_graphicsView = graphicsView;
			_gestureLayer = new GestureLayer(this);
			_gestureLayer.Attach(this);

			_gestureLayer.SetTapCallback(GestureLayer.GestureType.Tap, GestureLayer.GestureState.Start, (_) => { OnGestureStarted(); });

			_gestureLayer.SetTapCallback(GestureLayer.GestureType.Tap, GestureLayer.GestureState.End, (_) => { OnGestureEnded(true); });

			_gestureLayer.SetLineCallback(GestureLayer.GestureState.Start, (_) => { OnGestureStarted(); });

			_gestureLayer.SetLineCallback(GestureLayer.GestureState.Move, (_) =>
			{
				_graphicsView?.DragInteraction(new[] { _gestureLayer.EvasCanvas.Pointer.ToPointF() });
			});

			_gestureLayer.SetLineCallback(GestureLayer.GestureState.End, (_) =>
			{
				OnGestureEnded(Geometry.ToDP().Contains(_gestureLayer.EvasCanvas.Pointer.ToPoint()));
			});

			_gestureLayer.SetLineCallback(GestureLayer.GestureState.Abort, (_) =>
			{
				_graphicsView?.CancelInteraction();
			});
		}

		public void Disconnect()
		{
			_gestureLayer?.Unrealize();
			_gestureLayer = null;
			_graphicsView = null;
		}

		void OnGestureStarted()
		{
			if (_graphicsView is null || _gestureLayer is null)
				return;

			_graphicsView.StartInteraction(new[] { _gestureLayer.EvasCanvas.Pointer.ToPointF() });
		}

		void OnGestureEnded(bool isInsideBounds)
		{
			if (_graphicsView is null || _gestureLayer is null)
				return;

			_graphicsView.EndInteraction(new[] { _gestureLayer.EvasCanvas.Pointer.ToPointF() }, isInsideBounds);
		}
	}
}
