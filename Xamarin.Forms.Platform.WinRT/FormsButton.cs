using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

#if WINDOWS_UWP
using WContentPresenter = Windows.UI.Xaml.Controls.ContentPresenter;

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public class FormsButton : Windows.UI.Xaml.Controls.Button
	{
		public static readonly DependencyProperty BorderRadiusProperty = DependencyProperty.Register(nameof(BorderRadius), typeof(int), typeof(FormsButton),
			new PropertyMetadata(default(int), OnBorderRadiusChanged));

		public static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.Register(nameof(BackgroundColor), typeof(Brush), typeof(FormsButton),
			new PropertyMetadata(default(Brush), OnBackgroundColorChanged));

#if WINDOWS_UWP
		WContentPresenter _contentPresenter;
#else
		Border _border;
#endif

		public Brush BackgroundColor
		{
			get
			{
				return (Brush)GetValue(BackgroundColorProperty);
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

#if WINDOWS_UWP
			_contentPresenter = GetTemplateChild("ContentPresenter") as WContentPresenter;	
#else
			_border = GetTemplateChild("Border") as Border;
#endif
			UpdateBackgroundColor();
			UpdateBorderRadius();
		}

		static void OnBackgroundColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((FormsButton)d).UpdateBackgroundColor();
		}

		static void OnBorderRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((FormsButton)d).UpdateBorderRadius();
		}

		void UpdateBackgroundColor()
		{
			if (BackgroundColor == null)
				BackgroundColor = Background;

#if WINDOWS_UWP
			if (_contentPresenter != null)
				_contentPresenter.Background = BackgroundColor;
#else
			if (_border != null)
				_border.Background = BackgroundColor;
#endif
			Background = Color.Transparent.ToBrush();
		}

		void UpdateBorderRadius()
		{

#if WINDOWS_UWP
			if (_contentPresenter != null)
				_contentPresenter.CornerRadius = new CornerRadius(BorderRadius);
#else
			if (_border != null)
				_border.CornerRadius = new CornerRadius(BorderRadius);
#endif
		}
	}
}