using System;
using System.Diagnostics.CodeAnalysis;
using CoreAnimation;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using UIKit;
using static Microsoft.Maui.Primitives.Dimension;

namespace Microsoft.Maui.Platform
{
	public partial class WrapperView : UIView, IDisposable, IUIViewLifeCycleEvents
	{
		bool _fireSetNeedsLayoutOnParentWhenWindowAttached;
		WeakReference<ICrossPlatformLayout>? _crossPlatformLayoutReference;

		internal ICrossPlatformLayout? CrossPlatformLayout
		{
			get => _crossPlatformLayoutReference != null && _crossPlatformLayoutReference.TryGetTarget(out var v) ? v : null;
			set => _crossPlatformLayoutReference = value == null ? null : new WeakReference<ICrossPlatformLayout>(value);
		}

		double _lastMeasureHeight = double.NaN;
		double _lastMeasureWidth = double.NaN;

		CAShapeLayer? _maskLayer;
		CAShapeLayer? _backgroundMaskLayer;
		CAShapeLayer? _shadowLayer;
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "_borderView is a SubView")]
		UIView? _borderView;

		public WrapperView()
		{
		}

		public WrapperView(CGRect frame)
			: base(frame)
		{
		}

		internal bool IsMeasureValid(double widthConstraint, double heightConstraint)
		{
			// Check the last constraints this View was measured with; if they're the same,
			// then the current measure info is already correct and we don't need to repeat it
			return heightConstraint == _lastMeasureHeight && widthConstraint == _lastMeasureWidth;
		}

		internal void CacheMeasureConstraints(double widthConstraint, double heightConstraint)
		{
			_lastMeasureWidth = widthConstraint;
			_lastMeasureHeight = heightConstraint;
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

			var subviews = Subviews;
			if (subviews.Length == 0)
				return;

			if (_borderView is not null)
				BringSubviewToFront(_borderView);

			var child = subviews[0];

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

			var boundWidth = Bounds.Width;
			var boundHeight = Bounds.Height;

			if (!IsMeasureValid(boundWidth, boundHeight))
			{
				CrossPlatformLayout?.CrossPlatformMeasure(boundWidth, boundHeight);
				CacheMeasureConstraints(boundWidth, boundHeight);
			}

			CrossPlatformLayout?.CrossPlatformArrange(Bounds.ToRectangle());
		}

		internal void Disconnect()
		{
			MaskLayer = null;
			BackgroundMaskLayer = null;
			ShadowLayer = null;
			_borderView?.RemoveFromSuperview();
		}


		// TODO obsolete or delete this for NET9
		public new void Dispose()
		{
			Disconnect();
			base.Dispose();
		}

		public override CGSize SizeThatFits(CGSize size)
		{
			var subviews = Subviews;
			CGSize returnSize;

			if (subviews.Length == 0)
			{
				returnSize = base.SizeThatFits(size);
			}

			else
			{
				var child = subviews[0];

				// Calling SizeThatFits on an ImageView always returns the image's dimensions, so we need to call the extension method
				// This also affects ImageButtons
				if (child is UIImageView imageView)
				{
					returnSize = imageView.SizeThatFitsImage(size);
				}
				else if (CrossPlatformLayout is not null)
				{
					returnSize = CrossPlatformLayout.CrossPlatformMeasure(size.Width, size.Height).ToCGSize();
				}
				else if (child is UIButton imageButton && imageButton.ImageView?.Image is not null && imageButton.CurrentTitle is null)
				{
					returnSize = imageButton.ImageView.SizeThatFitsImage(size);
				}
				else
				{
					returnSize = child.SizeThatFits(size);
				}
			}

			CacheMeasureConstraints(size.Width, size.Height);
			return returnSize;
		}

		internal CGSize SizeThatFitsWrapper(CGSize originalSpec, double virtualViewWidth, double virtualViewHeight, IView view)
		{
			var subviews = Subviews;
			CGSize returnSize;
			var widthConstraint = IsExplicitSet(virtualViewWidth) ? virtualViewWidth : originalSpec.Width;
			var heightConstraint = IsExplicitSet(virtualViewHeight) ? virtualViewHeight : originalSpec.Height;

			if (subviews.Length == 0)
			{
				returnSize = base.SizeThatFits(originalSpec);
			}

			else
			{
				var child = subviews[0];

				if (child is UIImageView || (child is UIButton imageButton && imageButton.ImageView?.Image is not null && imageButton.CurrentTitle is null))
				{
					if (CrossPlatformLayout is not null)
					{
						returnSize = CrossPlatformLayout.CrossPlatformMeasure(widthConstraint, heightConstraint);
					}
					else
					{
						returnSize = SizeThatFits(new CGSize(widthConstraint, heightConstraint));
					}
				}

				else if (CrossPlatformLayout is not null)
				{
					returnSize = CrossPlatformLayout.CrossPlatformMeasure(widthConstraint, heightConstraint);
				}
				else
				{
					returnSize = SizeThatFits(originalSpec);
				}
			}

			CacheMeasureConstraints(widthConstraint, heightConstraint);
			return returnSize;
		}

		public override void SetNeedsLayout()
		{
			base.SetNeedsLayout();
			TryToInvalidateSuperView(false);
		}

		private protected void TryToInvalidateSuperView(bool onlyIfPending)
		{
			if (onlyIfPending && !_fireSetNeedsLayoutOnParentWhenWindowAttached)
			{
				return;
			}

			// We check for Window to avoid scenarios where an invalidate might propagate up the tree
			// To a SuperView that's been disposed which will cause a crash when trying to access it
			if (Window is not null)
			{
				_fireSetNeedsLayoutOnParentWhenWindowAttached = false;
				this.Superview?.SetNeedsLayout();
			}
			else
			{
				_fireSetNeedsLayoutOnParentWhenWindowAttached = true;
			}
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

			mask ??= MaskLayer = new StaticCAShapeLayer();
			mask.Path = nativePath;

			var backgroundLayer = GetBackgroundLayer();

			// We wrap some controls for certain visual effects like applying background gradient etc.
			// For this reason, we have to clip the background layer as well if it exists.
			if (backgroundLayer is null)
				return;

			backgroundMask ??= BackgroundMaskLayer = new StaticCAShapeLayer();
			backgroundMask.Path = nativePath;
		}

		void SetShadow()
		{
			var shadowLayer = ShadowLayer;

			if (shadowLayer == null && Shadow == null)
				return;

			shadowLayer ??= ShadowLayer = new StaticCAShapeLayer();

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

		CALayer? GetLayer()
		{
			var sublayers = Layer?.Sublayers;
			if (sublayers is null)
				return null;

			foreach (var subLayer in sublayers)
				if (subLayer.Delegate is not null)
					return subLayer;

			return Layer;
		}

		CALayer? GetBackgroundLayer()
		{
			var sublayers = Layer?.Sublayers;
			if (sublayers is null)
				return null;

			foreach (var subLayer in sublayers)
				if (subLayer.Name == ViewExtensions.BackgroundLayerName)
					return subLayer;

			return Layer;
		}

		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = IUIViewLifeCycleEvents.UnconditionalSuppressMessage)]
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
			TryToInvalidateSuperView(true);
		}
	}
}