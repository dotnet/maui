using System;
using CoreAnimation;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class ContentView : MauiView
	{
		WeakReference<IBorderStroke>? _clip;
		CAShapeLayer? _contentMask;

		// The BorderHandler tags its stable content container with this so the border mask
		// remains independent of transforms applied to the child view.
		internal const nint ContentTag = 0x63D2A0;

		public ContentView()
		{
			if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13, 1))
				Layer.CornerCurve = CACornerCurve.Continuous; // Available from iOS 13. More info: https://developer.apple.com/documentation/quartzcore/calayercornercurve/3152600-continuous
		}

		private protected ContentView(NativeHandle handle)
			: base(handle)
		{
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			UpdateClip();
		}

		internal IBorderStroke? Clip
		{
			get => _clip is not null && _clip.TryGetTarget(out var clip) ? clip : null;
			set
			{
				_clip = value is null ? null : new(value);

				if (value is not null)
				{
					UpdateClip();
				}
			}
		}

		UIView? PlatformContent
		{
			get
			{
				// It's a fair bet that Subviews[0] will always be the content for the ContentView
				// But just in case, we're going to iterate over the views and check the tag
				foreach (var subview in Subviews)
				{
					if (subview.Tag == ContentTag)
					{
						return subview;
					}
				}

				return null;
			}
		}

		void RemoveContentMask()
		{
			if (_contentMask is not null && _contentMask.Handle != IntPtr.Zero)
			{
				_contentMask.RemoveFromSuperLayer();
			}
			_contentMask = null;
		}

		void UpdateClip()
		{
			var contentContainer = PlatformContent;
			var clip = Clip;
			var bounds = Bounds;

			if (clip is null || bounds == CGRect.Empty || contentContainer is null || contentContainer.Frame == CGRect.Empty)
			{
				RemoveContentMask();
				return;
			}

			var containerFrame = contentContainer.Frame;
			_contentMask ??= new StaticCAShapeLayer();

			var strokeThickness = (float)clip.StrokeThickness;

			// We need to inset the content clipping by the width of the stroke on both sides
			// (top and bottom, left and right), so we remove it twice from the total width/height 
			var strokeInset = 2 * strokeThickness;
			var clipWidth = (float)bounds.Width - strokeInset;
			var clipHeight = (float)bounds.Height - strokeInset;

			var clipBounds = new RectF(0, 0, clipWidth, clipHeight);
			_contentMask.Path = GetClipPath(clipBounds, strokeThickness);

			// Since the mask is on the content container's CALayer, it's anchored to the container.
			// Compute its position relative to this border.

			var containerOffsetX = containerFrame.X;
			var containerOffsetY = containerFrame.Y;

			var clipBoundsCenter = clipBounds.Center;
			var clipCenterX = clipBoundsCenter.X + (strokeThickness);
			var clipCenterY = clipBoundsCenter.Y + (strokeThickness);

			CGPoint adjustedMaskPosition = new(clipCenterX - containerOffsetX, clipCenterY - containerOffsetY);

			_contentMask.Bounds = clipBounds;
			_contentMask.Position = adjustedMaskPosition;

			if (contentContainer.Layer.Mask != _contentMask)
			{
				contentContainer.Layer.Mask = _contentMask;
			}
		}

		CGPath? GetClipPath(RectF bounds, float strokeThickness)
		{
			IShape? clipShape = Clip?.Shape;
			PathF? path;

			if (clipShape is IRoundRectangle roundRectangle)
				path = roundRectangle.InnerPathForBounds(bounds, strokeThickness, includeShapeStroke: true);
			else
				path = clipShape is IShapeWithStroke shapeWithStroke
					? shapeWithStroke.PathForBounds(bounds, includeStroke: true)
					: clipShape?.PathForBounds(bounds);

			return path?.AsCGPath();
		}

		public override void WillRemoveSubview(UIView uiview)
		{
			// Make sure we're not holding a mask for content we no longer own
			if (uiview == PlatformContent)
			{
				RemoveContentMask();
			}

			base.WillRemoveSubview(uiview);
		}
	}
}
