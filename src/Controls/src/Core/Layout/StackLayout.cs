using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	public abstract class StackBase : Layout, IStackLayout
	{
		public static readonly BindableProperty SpacingProperty = BindableProperty.Create(nameof(Spacing), typeof(double), typeof(StackBase), 0d,
				propertyChanged: (bindable, oldvalue, newvalue) => ((IView)bindable).InvalidateMeasure());

		public double Spacing
		{
			get { return (double)GetValue(SpacingProperty); }
			set { SetValue(SpacingProperty, value); }
		}
	}

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

			// Clear out the current layout manager; when the layout system needs it again, it'll call CreateLayoutManager
			layout._layoutManager = null;
			layout.InvalidateMeasure();
		}

		protected override ILayoutManager CreateLayoutManager()
		{
			return Orientation switch
			{
				StackOrientation.Horizontal => new HorizontalStackLayoutManager(this),
				_ => new VerticalStackLayoutManager(this),
			};
		}
	}
}
