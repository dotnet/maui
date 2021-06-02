using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public class MauiActivityIndicator : ProgressBar
	{
		public static readonly DependencyProperty ElementOpacityProperty = DependencyProperty.Register(
			nameof(ElementOpacity), typeof(double), typeof(MauiActivityIndicator), new PropertyMetadata(default(double)));

		public double ElementOpacity
		{
			get => (double)GetValue(ElementOpacityProperty);
			set => SetValue(ElementOpacityProperty, value);
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
