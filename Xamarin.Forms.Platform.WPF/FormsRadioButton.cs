using System.Windows;
using Xamarin.Forms.Platform.WPF.Controls;
using WBrush = System.Windows.Media.Brush;

namespace Xamarin.Forms.Platform.WPF
{
	public class FormsRadioButton : System.Windows.Controls.RadioButton
	{
		public static readonly DependencyProperty BorderRadiusProperty = DependencyProperty.Register(nameof(BorderRadius), typeof(int), typeof(FormsButton),	
			new PropertyMetadata(default(int), OnBorderRadiusChanged));

		public static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.Register(nameof(BackgroundColor), typeof(WBrush), typeof(FormsButton),
			new PropertyMetadata(default(WBrush), OnBackgroundColorChanged));


		public WBrush BackgroundColor
		{
			get
			{
				return (WBrush)GetValue(BackgroundColorProperty);
			}
			set
			{
				SetValue(BackgroundColorProperty, value);
			}
		}

		public int BorderRadius
		{
			get
			{
				return (int)GetValue(BorderRadiusProperty);
			}
			set
			{
				SetValue(BorderRadiusProperty, value);
			}
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			UpdateBackgroundColor();
		}

		static void OnBackgroundColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((FormsRadioButton)d).UpdateBackgroundColor();
		}

		static void OnBorderRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{

		}

		void UpdateBackgroundColor()
		{
			if (BackgroundColor == null)
				BackgroundColor = Background;

			Background = BackgroundColor;
		}

	}
}
