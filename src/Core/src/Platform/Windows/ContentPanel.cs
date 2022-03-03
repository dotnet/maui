#nullable enable
using System;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Shapes;

namespace Microsoft.Maui.Platform
{
	public class ContentPanel : Panel
	{
		readonly Path? _borderPath;
		IShape? _borderShape;

		internal Path? BorderPath => _borderPath;
		internal Func<double, double, Size>? CrossPlatformMeasure { get; set; }
		internal Func<Graphics.Rect, Size>? CrossPlatformArrange { get; set; }

		protected override global::Windows.Foundation.Size MeasureOverride(global::Windows.Foundation.Size availableSize)
		{
			if (CrossPlatformMeasure == null)
			{
				return base.MeasureOverride(availableSize);
			}

			var measure = CrossPlatformMeasure(availableSize.Width, availableSize.Height);

			return measure.ToPlatform();
		}

		protected override global::Windows.Foundation.Size ArrangeOverride(global::Windows.Foundation.Size finalSize)
		{
			if (CrossPlatformArrange == null)
			{
				return base.ArrangeOverride(finalSize);
			}

			var width = finalSize.Width;
			var height = finalSize.Height;

			var actual = CrossPlatformArrange(new Graphics.Rect(0, 0, width, height));

			return new global::Windows.Foundation.Size(actual.Width, actual.Height);
		}

		public ContentPanel()
		{
			_borderPath = new Path();
			EnsureBorderPath();

			SizeChanged += ContentPanelSizeChanged;
		}

		private void ContentPanelSizeChanged(object sender, UI.Xaml.SizeChangedEventArgs e)
		{
			if (_borderPath == null)
				return;

			_borderPath.UpdatePath(_borderShape, ActualWidth, ActualHeight);
		}

		internal void EnsureBorderPath()
		{
			if (!Children.Contains(_borderPath))
			{
				Children.Add(_borderPath);
			}
		}

		public void UpdateBackground(Paint? background)
		{
			if (_borderPath == null)
				return;

			_borderPath.UpdateBackground(background);
		}

		public void UpdateBorderShape(IShape borderShape)
		{
			_borderShape = borderShape;

			if (_borderPath == null)
				return;

			_borderPath.UpdateBorderShape(_borderShape, ActualWidth, ActualHeight);
		}
	}
}