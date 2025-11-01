using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WContentPresenter = Microsoft.UI.Xaml.Controls.ContentPresenter;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public partial class FormsRadioButton : Microsoft.UI.Xaml.Controls.RadioButton
	{
		public static readonly DependencyProperty BorderRadiusProperty = DependencyProperty.Register(nameof(BorderRadius), typeof(int), typeof(FormsButton),
			new PropertyMetadata(default(int), OnBorderRadiusChanged));

		public static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.Register(nameof(BackgroundColor), typeof(WBrush), typeof(FormsButton),
			new PropertyMetadata(default(WBrush), OnBackgroundColorChanged));

		WContentPresenter _contentPresenter;

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

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_contentPresenter = GetTemplateChild("ContentPresenter") as WContentPresenter;

			UpdateBackgroundColor();
			UpdateBorderRadius();
		}

		static void OnBackgroundColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((FormsRadioButton)d).UpdateBackgroundColor();
		}

		static void OnBorderRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((FormsRadioButton)d).UpdateBorderRadius();
		}

		void UpdateBackgroundColor()
		{
			if (BackgroundColor == null)
				BackgroundColor = Background;

			_contentPresenter?.Background = BackgroundColor;
			Background = new UI.Xaml.Media.SolidColorBrush(UI.Colors.Transparent);
		}

		void UpdateBorderRadius()
		{
			_contentPresenter?.CornerRadius = WinUIHelpers.CreateCornerRadius(BorderRadius);
		}
	}
}