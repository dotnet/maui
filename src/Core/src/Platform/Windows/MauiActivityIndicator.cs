using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Platform
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

		protected override global::Windows.Foundation.Size MeasureOverride(global::Windows.Foundation.Size availableSize)
		{
			var result = base.MeasureOverride(availableSize);

			if (!double.IsInfinity(availableSize.Width))
				result.Width = availableSize.Width;

			return result;
		}
	}
}
