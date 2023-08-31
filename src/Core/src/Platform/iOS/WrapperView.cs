using System;
using System.Diagnostics.CodeAnalysis;
using CoreAnimation;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public partial class WrapperView : UIView, IDisposable, IUIViewLifeCycleEvents
	{
		CAShapeLayer? _maskLayer;
		CAShapeLayer? _backgroundMaskLayer;
		CAShapeLayer? _shadowLayer;
		[UnconditionalSuppressMessage("Memory", "MA0002", Justification = "_borderView is a SubView")]
		UIView? _borderView;

		public WrapperView()
		{
		}

		public WrapperView(CGRect frame)
			: base(frame)
		{
		}

		CAShapeLayer? MaskLayer
		{
			get => _maskLayer;
			set
			{
				var layer = GetLayer();

				if (layer is not null && _maskLayer is not null)
					layer.Mask = null;

				_maskLayer = value;

				if (layer is not null)
					layer.Mask = value;
			}
		}

		CAShapeLayer? BackgroundMaskLayer
		{
			get => _backgroundMaskLayer;
			set
			{
				var backgroundLayer = GetBackgroundLayer();

				if (backgroundLayer is not null && _backgroundMaskLayer is not null)
					backgroundLayer.Mask = null;

				_backgroundMaskLayer = value;

				if (backgroundLayer is not null)
					backgroundLayer.Mask = value;
			}
		}

		CAShapeLayer? ShadowLayer
		{
			get => _shadowLayer;
			set
			{
				_shadowLayer?.RemoveFromSuperLayer();
				_shadowLayer = value;

				if (_shadowLayer != null)
					Layer.InsertSublayer(_shadowLayer, 0);
			}
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			if (Subviews.Length == 0)
				return;

			if (_borderView is not null)
				BringSubviewToFront(_borderView);

			var child = Subviews[0];

			child.Frame = Bounds;

			if (MaskLayer is not null)
				MaskLayer.Frame = Bounds;

			if (BackgroundMaskLayer is not null)
				BackgroundMaskLayer.Frame = Bounds;

			if (ShadowLayer is not null)
				ShadowLayer.Frame = Bounds;

			if (_borderView is not null)
				_borderView.Frame = Bounds;

			SetClip();
			SetShadow();
			SetBorder();
		}

		public new void Dispose()
		{
			DisposeClip();
			DisposeShadow();
			DisposeBorder();

			base.Dispose();
		}

		public override CGSize SizeThatFits(CGSize size)
		{
			if (Subviews.Length == 0)
				return base.SizeThatFits(size);

			var child = Subviews[0];

			if (child is UIImageView imageView)
				return imageView.SizeThatFitsImage(size);

			return child.SizeThatFits(size);
		}

		public override void SetNeedsLayout()
		{
			base.SetNeedsLayout();

			Superview?.SetNeedsLayout();
		}

		partial void ClipChanged()
		{
			SetClip();
		}

		partial void ShadowChanged()
		{
			SetShadow();
		}

		partial void BorderChanged() => SetBorder();

		void SetClip()
		{
			var mask = MaskLayer;
			var backgroundMask = BackgroundMaskLayer;

			if (mask is null && Clip is null)
				return;

			var frame = Frame;
			var bounds = new RectF(0, 0, (float)frame.Width, (float)frame.Height);
			var path = _clip?.PathForBounds(bounds);
			var nativePath = path?.AsCGPath();

			mask ??= MaskLayer = new CAShapeLayer();
			mask.Path = nativePath;

			var backgroundLayer = GetBackgroundLayer();

			// We wrap some controls for certain visual effects like applying background gradient etc.
			// For this reason, we have to clip the background layer as well if it exists.
			if (backgroundLayer is null)
				return;

			backgroundMask ??= BackgroundMaskLayer = new CAShapeLayer();
			backgroundMask.Path = nativePath;
		}

		void DisposeClip()
		{
			MaskLayer = null;
			BackgroundMaskLayer = null;
		}

		void SetShadow()
		{
			var shadowLayer = ShadowLayer;

			if (shadowLayer == null && Shadow == null)
				return;

			shadowLayer ??= ShadowLayer = new CAShapeLayer();

			var frame = Frame;
			var bounds = new RectF(0, 0, (float)frame.Width, (float)frame.Height);

			shadowLayer.FillColor = new CGColor(0, 0, 0, 1);

			var path = _clip?.PathForBounds(bounds);
			var nativePath = path?.AsCGPath();
			shadowLayer.Path = nativePath;

			if (Shadow == null)
				shadowLayer.ClearShadow();
			else
				shadowLayer.SetShadow(Shadow);
		}

		void DisposeShadow()
		{
			ShadowLayer = null;
		}

		void SetBorder()
		{
			if (Border == null)
			{
				_borderView?.RemoveFromSuperview();
				return;
			}

			if (_borderView is null)
			{
				AddSubview(_borderView = new UIView(Bounds) { UserInteractionEnabled = false });
			}

			_borderView.UpdateMauiCALayer(Border);
		}

		void DisposeBorder()
		{
			_borderView?.RemoveFromSuperview();
		}

		CALayer? GetLayer()
		{
			if (Layer is null || Layer.Sublayers is null)
				return null;

			foreach (var subLayer in Layer.Sublayers)
				if (subLayer.Delegate is not null)
					return subLayer;

			return Layer;
		}

		CALayer? GetBackgroundLayer()
		{
			if (Layer is null || Layer.Sublayers is null)
				return null;

			foreach (var subLayer in Layer.Sublayers)
				if (subLayer.Name == ViewExtensions.BackgroundLayerName)
					return subLayer;

			return Layer;
		}

		[UnconditionalSuppressMessage("Memory", "MA0002", Justification = IUIViewLifeCycleEvents.UnconditionalSuppressMessage)]
		EventHandler? _movedToWindow;
		event EventHandler? IUIViewLifeCycleEvents.MovedToWindow
		{
			add => _movedToWindow += value;
			remove => _movedToWindow -= value;
		}

		public override void MovedToWindow()
		{
			base.MovedToWindow();
			_movedToWindow?.Invoke(this, EventArgs.Empty);
		}
	}
}