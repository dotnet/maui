using System.Windows;
using WBrush = System.Windows.Media.Brush;
using WPFCheckBox = System.Windows.Controls.CheckBox;
using WSolidColorBrush = System.Windows.Media.SolidColorBrush;

namespace Xamarin.Forms.Platform.WPF
{
	public class FormsCheckBox : WPFCheckBox
	{

		public static readonly DependencyProperty TintBrushProperty =
			DependencyProperty.Register(nameof(TintBrush), typeof(WBrush), typeof(FormsCheckBox),
				new PropertyMetadata(default(WBrush), OnTintBrushPropertyChanged));

		static void OnTintBrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var checkBox = (FormsCheckBox)d;

			if (e.NewValue is WSolidColorBrush solidBrush && solidBrush.Color.A == 0)
			{
				checkBox.BorderBrush = Color.Black.ToBrush();
			}
			else if (e.NewValue is WSolidColorBrush b)
			{
				checkBox.BorderBrush = b;
			}
		}

		public FormsCheckBox()
		{
			BorderBrush = Color.Black.ToBrush();
		}

		public WBrush TintBrush
		{
			get { return (WBrush)GetValue(TintBrushProperty); }
			set { SetValue(TintBrushProperty, value); }
		}
	}
}
