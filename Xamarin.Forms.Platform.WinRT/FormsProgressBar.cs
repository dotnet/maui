using Windows.UI.Xaml;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public class FormsProgressBar : Windows.UI.Xaml.Controls.ProgressBar
	{
		public static readonly DependencyProperty ElementOpacityProperty = DependencyProperty.Register(
			nameof(ElementOpacity), typeof(double), typeof(FormsProgressBar), new PropertyMetadata(default(double)));

		public double ElementOpacity
		{
			get { return (double)GetValue(ElementOpacityProperty); }
			set { SetValue(ElementOpacityProperty, value); }
		}
	}
}
