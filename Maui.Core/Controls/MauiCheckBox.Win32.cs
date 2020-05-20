using WCheckBox = System.Windows.Controls.CheckBox;
using WControl = System.Windows.Controls.Control;
using System.Windows;
using System.Windows.Media;
using System.Maui.Platform;

namespace System.Maui.Core.Controls
{
	public class MauiCheckBox : WCheckBox
	{

		public static readonly DependencyProperty TintBrushProperty =
			DependencyProperty.Register(nameof(TintBrush), typeof(Brush), typeof(MauiCheckBox),
				new PropertyMetadata(default(Brush), OnTintBrushPropertyChanged));

		static void OnTintBrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var checkBox = (MauiCheckBox)d;

			if (e.NewValue is SolidColorBrush solidBrush && solidBrush.Color.A == 0)
			{
				checkBox.BorderBrush = Color.Black.ToBrush();
			}
			else if (e.NewValue is SolidColorBrush b)
			{
				checkBox.BorderBrush = b;
			}
		}

		public MauiCheckBox()
		{
			BorderBrush = Color.Black.ToBrush();
		}

		public Brush TintBrush
		{
			get { return (Brush)GetValue(TintBrushProperty); }
			set { SetValue(TintBrushProperty, value); }
		}
	}
}
