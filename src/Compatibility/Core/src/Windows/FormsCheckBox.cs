using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WindowsCheckbox = Microsoft.UI.Xaml.Controls.CheckBox;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public class FormsCheckBox : WindowsCheckbox
	{
		public static readonly DependencyProperty TintBrushProperty =
			DependencyProperty.Register(nameof(TintBrush), typeof(WBrush), typeof(FormsCheckBox),
				new PropertyMetadata(default(WBrush), OnTintBrushPropertyChanged));

		static void OnTintBrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var checkBox =  (FormsCheckBox)d;

			if(checkBox.IsChecked == false)
			{
				checkBox.DefaultFillBrush = new UI.Xaml.Media.SolidColorBrush(UI.Colors.Transparent);
			}
			else
			{
				checkBox.DefaultFillBrush = (WBrush)e.NewValue;
			}
		}

		public static readonly DependencyProperty DefaultFillBrushProperty =
			DependencyProperty.Register(nameof(DefaultFillBrush), typeof(WBrush), typeof(FormsCheckBox),
				new PropertyMetadata(default(WBrush)));

		public FormsCheckBox()
		{
			
		}

		public WBrush TintBrush
		{
			get { return (WBrush)GetValue(TintBrushProperty); }
			set { SetValue(TintBrushProperty, value);  }
		}

		public WBrush DefaultFillBrush
		{
			get { return (WBrush)GetValue(DefaultFillBrushProperty); }
			set { SetValue(DefaultFillBrushProperty, value); }
		}
	}
}
