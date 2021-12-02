using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	public class StackLayout : StackBase, IStackLayout
	{
		public static readonly BindableProperty OrientationProperty = BindableProperty.Create(nameof(Orientation), typeof(StackOrientation), typeof(StackLayout), StackOrientation.Vertical,
			propertyChanged: OrientationChanged);

		public StackOrientation Orientation
		{
			get { return (StackOrientation)GetValue(OrientationProperty); }
			set { SetValue(OrientationProperty, value); }
		}

		static void OrientationChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var layout = (StackLayout)bindable;
			layout.InvalidateMeasure();
		}

		protected override ILayoutManager CreateLayoutManager()
		{
			return new StackLayoutManager(this);
		}
	}
}
