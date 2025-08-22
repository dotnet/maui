#nullable disable
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Primitives;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// A <see cref="Layout" /> that positions child elements in a single line which can be oriented vertically or horizontally.
	/// </summary>
	/// <remarks>
	/// Also see the specialized <see cref="VerticalStackLayout" /> and <see cref="HorizontalStackLayout" />, which might be more suitable if you do not need to change the orientation at runtime.
	/// </remarks>
	public class StackLayout : StackBase, IStackLayout
	{
		/// <summary>Bindable property for <see cref="Orientation"/>.</summary>
		public static readonly BindableProperty OrientationProperty = BindableProperty.Create(nameof(Orientation), typeof(StackOrientation), typeof(StackLayout), StackOrientation.Vertical,
			propertyChanged: OrientationChanged);

		/// <summary>
		/// Gets or sets the value which indicates the direction which child elements are positioned. Default value is <see cref="StackOrientation.Vertical"/>.
		/// This is a bindable property.
		/// </summary>
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

		protected override SizeConstraint ComputeConstraintForView(View view)
		{
			if (Orientation == StackOrientation.Horizontal)
			{
				if ((Constraint & SizeConstraint.VerticallyFixed) != 0 && view.VerticalOptions.Alignment == LayoutAlignment.Fill)
				{
					return SizeConstraint.VerticallyFixed;
				}
				else
				{
					return SizeConstraint.None;
				}
			}
			else
			{
				if ((Constraint & SizeConstraint.HorizontallyFixed) != 0 && view.HorizontalOptions.Alignment == LayoutAlignment.Fill)
				{
					return SizeConstraint.HorizontallyFixed;
				}
				else
				{
					return SizeConstraint.None;
				}
			}
		}
	}
}
