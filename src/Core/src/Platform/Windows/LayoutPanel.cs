#nullable enable
using System;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WSize = global::Windows.Foundation.Size;
using WRect = global::Windows.Foundation.Rect;

namespace Microsoft.Maui.Platform
{
	public class LayoutPanel : Panel
	{
		internal Func<double, double, Size>? CrossPlatformMeasure { get; set; }
		internal Func<Rectangle, Size>? CrossPlatformArrange { get; set; }

		public bool ClipsToBounds { get; set; }

		protected override WSize MeasureOverride(WSize availableSize)
		{
			if (CrossPlatformMeasure == null)
			{
				return base.MeasureOverride(availableSize);
			}

			var width = availableSize.Width;
			var height = availableSize.Height;

			var crossPlatformSize = CrossPlatformMeasure(width, height);

			width = crossPlatformSize.Width;
			height = crossPlatformSize.Height;

			return new WSize(width, height);
		}

		protected override WSize ArrangeOverride(WSize finalSize)
		{
			if (CrossPlatformArrange == null)
			{
				return base.ArrangeOverride(finalSize);
			}

			var width = finalSize.Width;
			var height = finalSize.Height;

			CrossPlatformArrange(new Rectangle(0, 0, width, height));

			Clip = ClipsToBounds ? new RectangleGeometry { Rect = new WRect(0, 0, finalSize.Width, finalSize.Height) } : null;

			return finalSize;
		}
	}
}
