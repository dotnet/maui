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
	public partial class WrapperView : UIView, IDisposable, IUIViewLifeCycleEvents, ICrossPlatformLayoutBacking, IPlatformMeasureInvalidationController
	{
		bool _invalidateParentWhenMovedToWindow;
		WeakReference<ICrossPlatformLayout>? _crossPlatformLayoutReference;

		ICrossPlatformLayout? ICrossPlatformLayoutBacking.CrossPlatformLayout
		{
			get => _crossPlatformLayoutReference != null && _crossPlatformLayoutReference.TryGetTarget(out var v) ? v : null;
			set => _crossPlatformLayoutReference = value == null ? null : new WeakReference<ICrossPlatformLayout>(value);
		}

		internal ICrossPlatformLayout? CrossPlatformLayout
		{
			get => ((ICrossPlatformLayoutBacking)this).CrossPlatformLayout;
			set => ((ICrossPlatformLayoutBacking)this).CrossPlatformLayout = value;
		}

		double _lastMeasureHeight = double.NaN;
		double _lastMeasureWidth = double.NaN;

		CAShapeLayer? _maskLayer;
		CAShapeLayer? _backgroundMaskLayer;
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
				var layer = GetContentLayer();

				if (layer is not null && _maskLayer is not null)
					layer.Mask = null;

				_maskLayer = value;

				layer?.Mask = value;
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

				backgroundLayer?.Mask = value;
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

			MaskLayer?.Frame = Bounds;

			BackgroundMaskLayer?.Frame = Bounds;

			_borderView?.Frame = Bounds;

			SetClip();
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

		partial void ClipChanged()
		{
			SetClip();
		}

		partial void BorderChanged() => SetBorder();

		void InvalidateConstraintsCache()
		{
			_lastMeasureWidth = double.NaN;
			_lastMeasureHeight = double.NaN;
		}

		void SetClip()
		{
			var clip = Clip;
			var mask = MaskLayer;
			var backgroundMask = BackgroundMaskLayer;

			if (mask is null && clip is null)
			{
				return;
			}

			if (clip is null)
			{
				MaskLayer = null;
				BackgroundMaskLayer = null;
				return;
			}

			var frame = Frame;
			var bounds = new RectF(0, 0, (float)frame.Width, (float)frame.Height);
			var path = clip.PathForBounds(bounds);
			var nativePath = path.AsCGPath();

			mask ??= MaskLayer = new StaticCAShapeLayer();
			mask.Path = nativePath;

			var backgroundLayer = GetBackgroundLayer();

			// We wrap some controls for certain visual effects like applying background gradient etc.
			// For this reason, we have to clip the background layer as well if it exists.
			if (backgroundLayer is null)
			{
				return;
			}

			backgroundMask ??= BackgroundMaskLayer = new StaticCAShapeLayer();
			backgroundMask.Path = nativePath;
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

		CALayer? GetContentLayer()
		{
			var subviews = Subviews;
			if (subviews.Length == 0)
				return null;

			return subviews[0].Layer;
		}

		CALayer? GetBackgroundLayer()
		{
			var sublayers = Layer?.Sublayers;
			if (sublayers is null)
				return null;

			foreach (var subLayer in sublayers)
				if (subLayer.Name == ViewExtensions.BackgroundLayerName)
					return subLayer;

			return null;
		}

		void IPlatformMeasureInvalidationController.InvalidateAncestorsMeasuresWhenMovedToWindow()
		{
			_invalidateParentWhenMovedToWindow = true;
		}

		bool IPlatformMeasureInvalidationController.InvalidateMeasure(bool isPropagating)
		{
			InvalidateConstraintsCache();
			SetNeedsLayout();
			return true;
		}

		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = IUIViewLifeCycleEvents.UnconditionalSuppressMessage)]
		EventHandler? _movedToWindow;
		event EventHandler? IUIViewLifeCycleEvents.MovedToWindow
		{
			remove => _movedToWindow -= value;
			add => _movedToWindow += value;
		}

		public override void MovedToWindow()
		{
			base.MovedToWindow();
			_movedToWindow?.Invoke(this, EventArgs.Empty);
			if (_invalidateParentWhenMovedToWindow)
			{
				_invalidateParentWhenMovedToWindow = false;
				this.InvalidateAncestorsMeasures();
			}
		}
	}
}