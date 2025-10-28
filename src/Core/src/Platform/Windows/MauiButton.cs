using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WRect = global::Windows.Foundation.Rect;
using WSize = global::Windows.Foundation.Size;
using WThickness = Microsoft.UI.Xaml.Thickness;

namespace Microsoft.Maui.Platform
{
	public partial class MauiButton : Button
	{
		public MauiButton()
		{
			Content = new DefaultMauiButtonContent();

			VerticalAlignment = VerticalAlignment.Stretch;
			HorizontalAlignment = HorizontalAlignment.Stretch;
		}

		protected override AutomationPeer OnCreateAutomationPeer()
		{
			return new MauiButtonAutomationPeer(this);
		}
	}

	internal partial class DefaultMauiButtonContent : MauiPanel
	{
		readonly Image _image;
		readonly TextBlock _textBlock;

		double _spacing;
		bool _isHorizontalLayout;
		bool _imageOnBottomOrRight;

		public DefaultMauiButtonContent()
		{
			HorizontalAlignment = HorizontalAlignment.Center;
			VerticalAlignment = VerticalAlignment.Center;
			Margin = new WThickness(0);

			_image = new Image
			{
				VerticalAlignment = VerticalAlignment.Stretch,
				HorizontalAlignment = HorizontalAlignment.Stretch,
				Stretch = Stretch.Uniform,
				Margin = new WThickness(0),
				Visibility = UI.Xaml.Visibility.Collapsed,
			};

			_textBlock = new TextBlock
			{
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center,
				Margin = new WThickness(0),
				Visibility = UI.Xaml.Visibility.Collapsed,
			};

			CachedChildren.Add(_image);
			CachedChildren.Add(_textBlock);

			LayoutImageLeft(0);
		}

		protected override WSize MeasureOverride(WSize availableSize)
		{
			double measuredHeight = 0;
			double measuredWidth = 0;
			double spacing = 0.0;

			// Always measure the image first, and use the remaining space for the text
			if (_image.Source != null &&
				_image.Visibility == UI.Xaml.Visibility.Visible)
			{
				_image.Measure(availableSize);
				measuredWidth = _image.DesiredSize.Width;
				measuredHeight = _image.DesiredSize.Height;
			}

			if (!string.IsNullOrEmpty(_textBlock.Text) &&
				_textBlock.Visibility == UI.Xaml.Visibility.Visible)
			{
				// Only add spacing if we have valid text
				spacing = _spacing;

				if (_isHorizontalLayout)
				{
					var availableWidth = Math.Max(0, availableSize.Width - measuredWidth - spacing);
					_textBlock.Measure(new WSize(availableWidth, availableSize.Height));

					measuredWidth += _textBlock.DesiredSize.Width;
					measuredHeight = Math.Max(measuredHeight, _textBlock.DesiredSize.Height);
				}
				else // Vertical
				{
					var availableHeight = Math.Max(0, availableSize.Height - measuredHeight - spacing);
					_textBlock.Measure(new WSize(availableSize.Width, availableHeight));

					measuredWidth = Math.Max(measuredWidth, _textBlock.DesiredSize.Width);
					measuredHeight += _textBlock.DesiredSize.Height;
				}
			}

			// Only add spacing if we have room
			if (_isHorizontalLayout)
			{
				measuredWidth = Math.Min(measuredWidth + spacing, availableSize.Width);
			}
			else // Vertical
			{
				measuredHeight = Math.Min(measuredHeight + spacing, availableSize.Height);
			}

			if (!double.IsInfinity(availableSize.Width) &&
				HorizontalAlignment == HorizontalAlignment.Stretch)
			{
				measuredWidth = Math.Max(measuredWidth, availableSize.Width);
			}

			if (!double.IsInfinity(availableSize.Height) &&
				VerticalAlignment == VerticalAlignment.Stretch)
			{
				measuredHeight = Math.Max(measuredHeight, availableSize.Height);
			}
			return new WSize(measuredWidth, measuredHeight);
		}

		protected override WSize ArrangeOverride(WSize finalSize)
		{
			if (_imageOnBottomOrRight)
			{
				ArrangeBottomAndRight(finalSize);
			}
			else
			{
				ArrangeLeftAndTop(finalSize);
			}

			return new WSize(finalSize.Width, finalSize.Height);
		}

		private void ArrangeLeftAndTop(WSize finalSize)
		{
			var x = 0.0;
			var y = 0.0;

			var spacing = _spacing;
			if (string.IsNullOrEmpty(_textBlock.Text))
			{
				spacing = 0;
			}

			if (_image.Visibility == UI.Xaml.Visibility.Visible)
			{
				var (newX, newY) = ArrangePrimaryElement(_image, x, y, spacing, finalSize);
				x = newX;
				y = newY;
			}

			if (!string.IsNullOrEmpty(_textBlock.Text) &&
				_textBlock.Visibility == UI.Xaml.Visibility.Visible)
			{
				ArrangeSecondaryElement(_textBlock, x, y, finalSize);
			}
		}

		private void ArrangeBottomAndRight(WSize finalSize)
		{
			var x = 0.0;
			var y = 0.0;

			if (!string.IsNullOrEmpty(_textBlock.Text) &&
				_textBlock.Visibility == UI.Xaml.Visibility.Visible)
			{
				var (newX, newY) = ArrangePrimaryElement(_textBlock, x, y, _spacing, finalSize);
				x = newX;
				y = newY;
			}

			if (_image.Visibility == UI.Xaml.Visibility.Visible)
			{
				ArrangeSecondaryElement(_image, x, y, finalSize);
			}
		}

		/// <summary>
		/// Arrange and center primary element (image or text) and return the new x or y position
		/// based on the element's size and if we're horizontal or vertical
		/// </summary>
		/// <param name="element"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="spacing"></param>
		/// <param name="finalSize"></param>
		/// <returns></returns>
		private (double newX, double newY) ArrangePrimaryElement(FrameworkElement element, double x, double y, double spacing, WSize finalSize)
		{
			if (_isHorizontalLayout)
			{
				var centeredY = Math.Max(0, (finalSize.Height / 2.0) - (element.DesiredSize.Height / 2.0));

				element.Arrange(new WRect(0, centeredY,
					element.DesiredSize.Width, element.DesiredSize.Height));

				return (x + element.DesiredSize.Width + spacing, 0);
			}
			else // Vertical
			{
				var centeredX = Math.Max(0, (finalSize.Width / 2.0) - (element.DesiredSize.Width / 2.0));

				element.Arrange(new WRect(centeredX, 0,
					element.DesiredSize.Width, element.DesiredSize.Height));

				return (0, y + element.DesiredSize.Height + spacing);
			}
		}

		private void ArrangeSecondaryElement(FrameworkElement element, double x, double y, WSize finalSize)
		{
			if (_isHorizontalLayout)
			{
				y = Math.Max(0, (finalSize.Height / 2.0) - (element.DesiredSize.Height / 2.0));
			}
			else
			{
				x = Math.Max(0, (finalSize.Width / 2.0) - (element.DesiredSize.Width / 2.0));
			}

			element.Arrange(new WRect(x, y,
				element.DesiredSize.Width, element.DesiredSize.Height));
		}

		public void LayoutImageLeft(double spacing)
		{
			_imageOnBottomOrRight = false;
			SetupHorizontalLayout(spacing);
		}

		public void LayoutImageRight(double spacing)
		{
			_imageOnBottomOrRight = true;
			SetupHorizontalLayout(spacing);
		}

		public void LayoutImageTop(double spacing)
		{
			_imageOnBottomOrRight = false;
			SetupVerticalLayout(spacing);
		}

		public void LayoutImageBottom(double spacing)
		{
			_imageOnBottomOrRight = true;
			SetupVerticalLayout(spacing);
		}

		double AdjustSpacing(double spacing)
		{
			if (_image.Visibility == UI.Xaml.Visibility.Collapsed
				|| _textBlock.Visibility == UI.Xaml.Visibility.Collapsed)
			{
				return 0;
			}

			return spacing;
		}

		void SetupHorizontalLayout(double spacing)
		{
			_isHorizontalLayout = true;
			_spacing = AdjustSpacing(spacing);
		}

		void SetupVerticalLayout(double spacing)
		{
			_isHorizontalLayout = false;
			_spacing = AdjustSpacing(spacing);
		}
	}
}