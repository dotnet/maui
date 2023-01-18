using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class ContentPropertyUnitTest : BaseTestFixture
	{
		[Theory]
		[InlineData(typeof(Border), "Content")]
		[InlineData(typeof(CarouselPage), "Children")]
		[InlineData(typeof(Compatibility.AbsoluteLayout), "Children")]
		[InlineData(typeof(Compatibility.FlexLayout), "Children")]
		[InlineData(typeof(Compatibility.Grid), "Children")]
		[InlineData(typeof(Compatibility.Layout<View>), "Children")]
		[InlineData(typeof(Compatibility.RelativeLayout), "Children")]
		[InlineData(typeof(Compatibility.StackLayout), "Children")]
		[InlineData(typeof(ContentPage), "Content")]
		[InlineData(typeof(ContentView), "Content")]
		[InlineData(typeof(DataTrigger), "Setters")]
		[InlineData(typeof(EventTrigger), "Actions")]
		[InlineData(typeof(FlexLayout), "Children")]
		[InlineData(typeof(FlyoutPage), "Detail")]
		[InlineData(typeof(FormattedString), "Spans")]
		[InlineData(typeof(Frame), "Content")]
		[InlineData(typeof(GradientBrush), "GradientStops")]
		[InlineData(typeof(Grid), "Children")]
		[InlineData(typeof(HorizontalStackLayout), "Children")]
		[InlineData(typeof(IndicatorView), "IndicatorLayout")]
		[InlineData(typeof(Label), "Text")]
		[InlineData(typeof(Layout), "Children")]
		[InlineData(typeof(MultiBinding), "Bindings")]
		[InlineData(typeof(MultiPage<Page>), "Children")]
		[InlineData(typeof(MultiTrigger), "Setters")]
		[InlineData(typeof(On), "Value")]
		[InlineData(typeof(OnPlatform<object>), "Platforms")]
		[InlineData(typeof(RadioButton), "Content")]
		[InlineData(typeof(RefreshView), "Content")]
		[InlineData(typeof(ScrollView), "Content")]
		[InlineData(typeof(Setter), "Value")]
		[InlineData(typeof(Shapes.GeometryGroup), "Children")]
		[InlineData(typeof(Shapes.Path), "Data")]
		[InlineData(typeof(Shapes.PathFigure), "Segments")]
		[InlineData(typeof(Shapes.PathGeometry), "Figures")]
		[InlineData(typeof(Shapes.Polygon), "Points")]
		[InlineData(typeof(Shapes.Polyline), "Points")]
		[InlineData(typeof(Shapes.TransformGroup), "Children")]
		[InlineData(typeof(Shell), "Items")]
		[InlineData(typeof(ShellContent), "Content")]
		[InlineData(typeof(ShellItem), "Items")]
		[InlineData(typeof(ShellSection), "Items")]
		[InlineData(typeof(Span), "Text")]
		[InlineData(typeof(Style), "Setters")]
		[InlineData(typeof(SwipeItemView), "Content")]
		[InlineData(typeof(SwipeView), "Content")]
		[InlineData(typeof(TabbedPage), "Children")]
		[InlineData(typeof(TableView), "Root")]
		[InlineData(typeof(Trigger), "Setters")]
		[InlineData(typeof(VerticalStackLayout), "Children")]
		[InlineData(typeof(ViewCell), "View")]
		[InlineData(typeof(VisualState), "Setters")]
		[InlineData(typeof(VisualStateGroup), "States")]
		[InlineData(typeof(Window), "Page")]
		public void CheckContentPropertyAttribute(Type type, string contentPropertyName)
		{
			var attributes = Attribute.GetCustomAttributes(type);
			var attribute = attributes.FirstOrDefault(e => e is ContentPropertyAttribute) as ContentPropertyAttribute;
			var hasValidContentProperty = attribute != null && attribute.Name.Equals(contentPropertyName, StringComparison.Ordinal);

			Assert.True(hasValidContentProperty, $@"The {type.FullName} class has an invalid ContentPropertyAttribute. It must be [ContentProperty(""{contentPropertyName}"")]");

			var implementsIEnumerable = typeof(IEnumerable).IsAssignableFrom(type);
			Assert.True(implementsIEnumerable, $"Class {type.FullName} must implement the IEnumerable interface");
		}
	}
}