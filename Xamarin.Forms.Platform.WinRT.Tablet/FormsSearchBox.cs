using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Xamarin.Forms.Platform.WinRT
{
	public class FormsSearchBox : SearchBox
	{
		public static readonly DependencyProperty HorizontalTextAlignmentProperty = DependencyProperty.Register(
			"HorizontalTextAlignment", typeof (Windows.UI.Xaml.TextAlignment), typeof (FormsSearchBox),
			new PropertyMetadata(Windows.UI.Xaml.TextAlignment.Left));

		public Windows.UI.Xaml.TextAlignment HorizontalTextAlignment
		{
			get { return (Windows.UI.Xaml.TextAlignment)GetValue(HorizontalTextAlignmentProperty); }
			set { SetValue(HorizontalTextAlignmentProperty, value); }
		}
	}
}