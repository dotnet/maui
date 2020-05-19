using Windows.UI.Xaml;

namespace Xamarin.Forms.Platform.UWP
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

		protected override Windows.Foundation.Size MeasureOverride(Windows.Foundation.Size availableSize)
		{
			var result = base.MeasureOverride(availableSize);
			if (!double.IsInfinity(availableSize.Width))
				result.Width = availableSize.Width;
			return result;
		}
	}
}
