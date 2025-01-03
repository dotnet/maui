#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;
using WSize = global::Windows.Foundation.Size;

namespace Microsoft.Maui.Platform
{
	public abstract class MauiPanel : Panel, ICrossPlatformLayoutBacking, IVisualTreeElementProvidable
	{
		UIElementCollection? _cachedChildren;

		[SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs", Justification = "Panel.Children property is banned to enforce use of this CachedChildren property.")]
		internal UIElementCollection CachedChildren
		{
			get
			{
				_cachedChildren ??= Children;
				return _cachedChildren;
			}
		}

		public ICrossPlatformLayout? CrossPlatformLayout
		{
			get; set;
		}

		Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			return CrossPlatformLayout?.CrossPlatformMeasure(widthConstraint, heightConstraint) ?? Size.Zero;
		}

		Size CrossPlatformArrange(Rect bounds)
		{
			return CrossPlatformLayout?.CrossPlatformArrange(bounds) ?? Size.Zero;
		}

		// TODO: Possibly reconcile this code with ViewHandlerExtensions.MeasureVirtualView
		// If you make changes here please review if those changes should also
		// apply to ViewHandlerExtensions.MeasureVirtualView
		protected override WSize MeasureOverride(global::Windows.Foundation.Size availableSize)
		{
			if (CrossPlatformMeasure == null || (availableSize.Width * availableSize.Height == 0))
			{
				return base.MeasureOverride(availableSize);
			}

			var measure = CrossPlatformMeasure(availableSize.Width, availableSize.Height);

			measure.Width = Math.Max(measure.Width, 0);
			measure.Height = Math.Max(measure.Height, 0);

			return measure.ToPlatform();
		}

		protected override WSize ArrangeOverride(WSize finalSize)
		{
			if (CrossPlatformArrange == null)
			{
				return base.ArrangeOverride(finalSize);
			}

			var width = finalSize.Width;
			var height = finalSize.Height;

			var actual = CrossPlatformArrange(new Rect(0, 0, width, height));

			return actual.ToPlatform();
		}

		IVisualTreeElement? IVisualTreeElementProvidable.GetElement()
		{
			if (CrossPlatformLayout is IVisualTreeElement layoutElement &&
				layoutElement.IsThisMyPlatformView(this))
			{
				return layoutElement;
			}

			return null;
		}
	}
}
