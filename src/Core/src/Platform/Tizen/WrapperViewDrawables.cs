using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{
	public class WrapperViewDrawables : IWrapperViewDrawables
	{
		public event EventHandler? Invalidated;

		IDrawable? _shadowDrawable;
		IDrawable? _backgroundDrawable;
		IDrawable? _borderDrawable;

		public IDrawable? ShadowDrawable
		{
			get
			{
				return _shadowDrawable;
			}
			set
			{
				_shadowDrawable = value;
				SendInvalidated();
			}
		}

		public IDrawable? BackgroundDrawable
		{
			get
			{
				return _backgroundDrawable;
			}
			set
			{
				_backgroundDrawable = value;
				SendInvalidated();
			}
		}

		public IDrawable? BorderDrawable
		{
			get
			{
				return _borderDrawable;
			}
			set
			{
				_borderDrawable = value;
				SendInvalidated();
			}
		}

		public void Draw(ICanvas canvas, RectF dirtyRect)
		{
			_shadowDrawable?.Draw(canvas, dirtyRect);
			_backgroundDrawable?.Draw(canvas, dirtyRect);
			_borderDrawable?.Draw(canvas, dirtyRect);
		}

		public void SendInvalidated()
		{
			Invalidated?.Invoke(this, EventArgs.Empty);
		}
	}
}
