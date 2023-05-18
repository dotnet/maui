#nullable disable
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/StackLayout.xml" path="Type[@FullName='Microsoft.Maui.Controls.StackLayout']/Docs/*" />
	public class StackLayout : StackBase, IStackLayout
	{
		/// <summary>Bindable property for <see cref="Orientation"/>.</summary>
		public static readonly BindableProperty OrientationProperty = BindableProperty.Create(nameof(Orientation), typeof(StackOrientation), typeof(StackLayout), StackOrientation.Vertical,
			propertyChanged: OrientationChanged);

		/// <include file="../../../docs/Microsoft.Maui.Controls/StackLayout.xml" path="//Member[@MemberName='Orientation']/Docs/*" />
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
