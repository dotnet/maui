using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public class FormsButton : Windows.UI.Xaml.Controls.Button
	{
		public static readonly DependencyProperty BorderRadiusProperty = DependencyProperty.Register("BorderRadius", typeof(int), typeof(FormsButton),
			new PropertyMetadata(default(int), OnBorderRadiusChanged));

		Border _border;

		public int BorderRadius
		{
			get { return (int)GetValue(BorderRadiusProperty); }
			set { SetValue(BorderRadiusProperty, value); }
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_border = GetTemplateChild("Border") as Border;
			UpdateBorderRadius();
		}

		static void OnBorderRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((FormsButton)d).UpdateBorderRadius();
		}

		void UpdateBorderRadius()
		{
			if (_border == null)
				return;

			_border.CornerRadius = new CornerRadius(BorderRadius);
		}
	}
}