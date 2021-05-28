using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WBrush = Microsoft.UI.Xaml.Media.Brush;

namespace Microsoft.Maui
{
	public class MauiCheckBox : CheckBox
	{
		public static readonly DependencyProperty TintBrushProperty =
			DependencyProperty.Register(nameof(TintBrush), typeof(WBrush), typeof(MauiCheckBox),
				new PropertyMetadata(default(WBrush), OnTintBrushPropertyChanged));

		static void OnTintBrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var checkBox = (MauiCheckBox)d;

			if (checkBox.IsChecked == false)
			{
				checkBox.DefaultFillBrush = new UI.Xaml.Media.SolidColorBrush(UI.Colors.Transparent);
			}
			else
			{
				checkBox.DefaultFillBrush = (WBrush)e.NewValue;
			}
		}

		public static readonly DependencyProperty DefaultFillBrushProperty =
			DependencyProperty.Register(nameof(DefaultFillBrush), typeof(WBrush), typeof(MauiCheckBox),
				new PropertyMetadata(default(WBrush)));

		public WBrush TintBrush
		{
			get { return (WBrush)GetValue(TintBrushProperty); }
			set { SetValue(TintBrushProperty, value); }
		}

		public WBrush DefaultFillBrush
		{
			get { return (WBrush)GetValue(DefaultFillBrushProperty); }
			set { SetValue(DefaultFillBrushProperty, value); }
		}
	}
}
