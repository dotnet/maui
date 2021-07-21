// This is a temporary namespace until we rename everything and move the legacy layouts
namespace Microsoft.Maui.Controls.Layout2
{
	public abstract class StackLayout : Layout, IStackLayout
	{
		public static readonly BindableProperty SpacingProperty = BindableProperty.Create(nameof(Spacing), typeof(double), typeof(StackLayout), 0d,
					propertyChanged: (bindable, oldvalue, newvalue) => ((IFrameworkElement)bindable).InvalidateMeasure());

		public double Spacing
		{
			get { return (double)GetValue(SpacingProperty); }
			set { SetValue(SpacingProperty, value); }
		}
	}
}
