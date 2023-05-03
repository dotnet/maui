#nullable disable
namespace Microsoft.Maui.Controls
{
	public abstract class StackBase : Layout, IStackLayout
	{
		/// <summary>Bindable property for <see cref="Spacing"/>.</summary>
		public static readonly BindableProperty SpacingProperty = BindableProperty.Create(nameof(Spacing), typeof(double), typeof(StackBase), 0d,
				propertyChanged: (bindable, oldvalue, newvalue) => ((IView)bindable).InvalidateMeasure());

		public double Spacing
		{
			get { return (double)GetValue(SpacingProperty); }
			set { SetValue(SpacingProperty, value); }
		}
	}
}
