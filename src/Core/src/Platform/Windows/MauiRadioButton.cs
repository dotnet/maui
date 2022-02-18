#nullable disable
using Microsoft.UI.Xaml;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WGrid = Microsoft.UI.Xaml.Controls.Grid;

namespace Microsoft.Maui.Platform
{
	public class MauiRadioButton : UI.Xaml.Controls.RadioButton
	{
		public static readonly DependencyProperty BorderRadiusProperty = DependencyProperty.Register(nameof(BorderRadius), typeof(int), typeof(MauiRadioButton),
			new PropertyMetadata(default(int), OnBorderRadiusChanged));

		public static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.Register(nameof(BackgroundColor), typeof(WBrush), typeof(MauiRadioButton),
			new PropertyMetadata(default(WBrush), OnBackgroundColorChanged));

		WGrid _contentPresenter;

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

			_contentPresenter = GetTemplateChild("RootGrid") as WGrid;

			UpdateBackgroundColor();
			UpdateBorderRadius();
		}

		static void OnBackgroundColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((MauiRadioButton)d).UpdateBackgroundColor();
		}

		static void OnBorderRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((MauiRadioButton)d).UpdateBorderRadius();
		}

		void UpdateBackgroundColor()
		{
			if (BackgroundColor == null)
				BackgroundColor = Background;

			if (_contentPresenter != null)
				_contentPresenter.Background = BackgroundColor;

			Background = new UI.Xaml.Media.SolidColorBrush(UI.Colors.Transparent);
		}

		void UpdateBorderRadius()
		{
			if (_contentPresenter != null)
				_contentPresenter.CornerRadius = WinUIHelpers.CreateCornerRadius(BorderRadius);
		}
	}
}