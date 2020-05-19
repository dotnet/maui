using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFCheckBox = System.Windows.Controls.CheckBox;
using WControl = System.Windows.Controls.Control;
using System.Windows;
using System.Windows.Media;

namespace System.Maui.Platform.WPF
{
	public class FormsCheckBox : WPFCheckBox
	{

		public static readonly DependencyProperty TintBrushProperty =
			DependencyProperty.Register(nameof(TintBrush), typeof(Brush), typeof(FormsCheckBox),
				new PropertyMetadata(default(Brush), OnTintBrushPropertyChanged));

		static void OnTintBrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var checkBox = (FormsCheckBox)d;

			if (e.NewValue is SolidColorBrush solidBrush && solidBrush.Color.A == 0)
			{
				checkBox.BorderBrush = Color.Black.ToBrush();
			}
			else if (e.NewValue is SolidColorBrush b)
			{
				checkBox.BorderBrush = b;
			}
		}

		public FormsCheckBox()
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
