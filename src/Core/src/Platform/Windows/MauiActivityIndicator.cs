using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Platform
{
	public class MauiActivityIndicator : ProgressRing
	{
		//public static readonly DependencyProperty ElementOpacityProperty = DependencyProperty.Register(
		//	nameof(ElementOpacity), typeof(double), typeof(MauiActivityIndicator), new PropertyMetadata(default(double)));

		//public double ElementOpacity
		//{
		//	get => (double)GetValue(ElementOpacityProperty);
		//	set => SetValue(ElementOpacityProperty, value);
		//}

		protected override global::Windows.Foundation.Size MeasureOverride(global::Windows.Foundation.Size availableSize)
		{
			if (double.IsInfinity(availableSize.Width))
				availableSize.Width = 32;

			if (double.IsInfinity(availableSize.Height))
				availableSize.Height = 32;

			var result = base.MeasureOverride(availableSize);

			return result;
		}
	}
}
