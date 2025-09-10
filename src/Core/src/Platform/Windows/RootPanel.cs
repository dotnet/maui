#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Platform
{
	[Obsolete("Use Microsoft.Maui.Platform.ContentPanel")]
	public partial class RootPanel : Panel
	{
		internal Func<double, double, Size>? CrossPlatformMeasure { get; set; }
		internal Func<Rect, Size>? CrossPlatformArrange { get; set; }

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

		protected override global::Windows.Foundation.Size MeasureOverride(global::Windows.Foundation.Size availableSize)
		{
			var width = availableSize.Width;
			var height = availableSize.Height;

			if (double.IsInfinity(width))
			{
				width = XamlRoot.Size.Width;
			}

			if (double.IsInfinity(height))
			{
				height = XamlRoot.Size.Height;
			}

			var size = new global::Windows.Foundation.Size(width, height);

			// Measure the children (should only be one, the Page)
			foreach (var child in CachedChildren)
			{
				child.Measure(size);
			}

			return size;
		}

		protected override global::Windows.Foundation.Size ArrangeOverride(global::Windows.Foundation.Size finalSize)
		{
			foreach (var child in CachedChildren)
			{
				child.Arrange(new global::Windows.Foundation.Rect(new global::Windows.Foundation.Point(0, 0), finalSize));
			}

			return finalSize;
		}
	}
}