using System;
using CoreAnimation;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class ContentView : MauiView
	{
		WeakReference<IBorderStroke>? _clip;
		CAShapeLayer? _contentMask;

		// When the BorderHandler sets the content UIView, it tags it with this so we can 
		// verify we're using the correct subview for masking (and any other purposes)
		internal const nint ContentTag = 0x63D2A0;

		public ContentView()
		{
			if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13, 1))
				Layer.CornerCurve = CACornerCurve.Continuous; // Available from iOS 13. More info: https://developer.apple.com/documentation/quartzcore/calayercornercurve/3152600-continuous
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
			_contentMask?.RemoveFromSuperLayer();
			_contentMask = null;
		}

		void UpdateClip()
		{
			var content = PlatformContent;

			if (Clip is null || Bounds == CGRect.Empty || content == null || content.Frame == CGRect.Empty)
			{
				RemoveContentMask();
				return;
			}

			_contentMask ??= new StaticCAShapeLayer();

			var bounds = Bounds;

			var strokeThickness = (float)Clip.StrokeThickness;

			// We need to inset the content clipping by the width of the stroke on both sides
			// (top and bottom, left and right), so we remove it twice from the total width/height 
			var strokeInset = 2 * strokeThickness;
			var clipWidth = (float)bounds.Width - strokeInset;
			var clipHeight = (float)bounds.Height - strokeInset;

			var clipBounds = new RectF(0, 0, clipWidth, clipHeight);
			_contentMask.Path = GetClipPath(clipBounds, strokeThickness);

			// Since the mask is on the content's CALayer, it's anchored to the content. But we need it to be
			// relative to _this_ container. So we need to compute an adjusted position for it.

			var contentFrame = content.Frame;
			var contentOffsetX = contentFrame.X;
			var contentOffsetY = contentFrame.Y;

			var clipBoundsCenter = clipBounds.Center;
			var clipCenterX = clipBoundsCenter.X + (strokeThickness);
			var clipCenterY = clipBoundsCenter.Y + (strokeThickness);

			CGPoint adjustedMaskPosition = new(clipCenterX - contentOffsetX, clipCenterY - contentOffsetY);

			_contentMask.Bounds = clipBounds;
			_contentMask.Position = adjustedMaskPosition;

			// Set the mask on the content, if it isn't already
			if (content.Layer.Mask != _contentMask)
			{
				content.Layer.Mask = _contentMask;
			}
		}

		CGPath? GetClipPath(RectF bounds, float strokeThickness)
		{
			IShape? clipShape = Clip?.Shape;
			PathF? path;

			if (clipShape is IRoundRectangle roundRectangle)
				path = roundRectangle.InnerPathForBounds(bounds, strokeThickness);
			else
				path = clipShape?.PathForBounds(bounds);

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