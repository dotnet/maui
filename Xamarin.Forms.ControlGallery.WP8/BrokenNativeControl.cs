using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Xamarin.Forms.ControlGallery.WP8
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

			Background = new RadialGradientBrush (Colors.Green, Colors.Blue);
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

		protected override System.Windows.Size ArrangeOverride (System.Windows.Size finalSize)
		{
			_textBlock.Arrange (new Rect (0, 0, finalSize.Width, finalSize.Height));
			return finalSize;
		}

		protected override System.Windows.Size MeasureOverride (System.Windows.Size availableSize)
		{
			_textBlock.Measure (availableSize);

			// This deliberately does something wrong so we can demo fixing it
			return new System.Windows.Size (600, _textBlock.DesiredSize.Height);
		}
	}
}