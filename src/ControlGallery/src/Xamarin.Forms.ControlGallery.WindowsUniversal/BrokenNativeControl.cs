using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI;
using Windows.UI.ViewManagement;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;

namespace Xamarin.Forms.ControlGallery.WindowsUniversal
{
	internal class BrokenNativeControl : Panel
	{
		public BrokenNativeControl ()
		{
			_textBlock = new TextBlock {
				MinHeight = 0,
				MaxHeight = double.PositiveInfinity,
				MinWidth = 0,
				MaxWidth = double.PositiveInfinity,
				FontSize = 24,
				HorizontalAlignment = HorizontalAlignment.Center
			};

			Children.Add (_textBlock);

			Background =
				new Microsoft.UI.Xaml.Media.LinearGradientBrush (
					new Microsoft.UI.Xaml.Media.GradientStopCollection { new Microsoft.UI.Xaml.Media.GradientStop { Color = Colors.Green, Offset = 0.5}, new Microsoft.UI.Xaml.Media.GradientStop { Color = Colors.Blue, Offset = 1} }, 0);
		}

		public static readonly DependencyProperty TextProperty = DependencyProperty.Register (
			"Text", typeof(string), typeof(BrokenNativeControl), new PropertyMetadata (default(string), PropertyChangedCallback));

		static void PropertyChangedCallback (DependencyObject dependencyObject,
			DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{
			((BrokenNativeControl)dependencyObject)._textBlock.Text = (string)dependencyPropertyChangedEventArgs.NewValue;
		}

		public string Text
		{
			get { return (string)GetValue (TextProperty); }
			set { SetValue (TextProperty, value); }
		}

		readonly TextBlock _textBlock;

		protected override Windows.Foundation.Size ArrangeOverride(Windows.Foundation.Size finalSize)
		{
			_textBlock.Arrange(new Windows.Foundation.Rect(0, 0, finalSize.Width, finalSize.Height));
			return finalSize;
		}


		protected override Windows.Foundation.Size  MeasureOverride (Windows.Foundation.Size availableSize)
		{
			_textBlock.Measure (availableSize);

			// This deliberately does something wrong so we can demo fixing it
			var bounds = ApplicationView.GetForCurrentView ().VisibleBounds;
			double scaleFactor = DisplayInformation.GetForCurrentView ().RawPixelsPerViewPixel;
			var size = new Size (bounds.Width * scaleFactor, bounds.Height * scaleFactor);

			return new Windows.Foundation.Size (size.Width, _textBlock.DesiredSize.Height);
		}
	}
}