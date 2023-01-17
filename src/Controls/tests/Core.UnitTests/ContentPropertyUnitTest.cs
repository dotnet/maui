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
		static (Type ClassType, string ContentPropertyName)[] classesWithContentPropertyNames = new[]
		{
			(typeof(Border), "Content"),
			(typeof(CarouselPage), "Children"),
			(typeof(Compatibility.AbsoluteLayout), "Children"),
			(typeof(Compatibility.FlexLayout), "Children"),
			(typeof(Compatibility.Grid), "Children"),
			(typeof(Compatibility.Layout<View>), "Children"),
			(typeof(Compatibility.RelativeLayout), "Children"),
			(typeof(Compatibility.StackLayout), "Children"),
			(typeof(ContentPage), "Content"),
			(typeof(ContentView), "Content"),
			(typeof(DataTrigger), "Setters"),
			(typeof(EventTrigger), "Actions"),
			(typeof(FlexLayout), "Children"),
			(typeof(FlyoutPage), "Detail"),
			(typeof(FormattedString), "Spans"),
			(typeof(Frame), "Content"),
			(typeof(GradientBrush), "GradientStops"),
			(typeof(Grid), "Children"),
			(typeof(HorizontalStackLayout), "Children"),
			(typeof(ImageBrush), "ImageSource"),
			(typeof(IndicatorView), "IndicatorLayout"),
			(typeof(Label), "Text"),
			(typeof(Layout), "Children"),
			(typeof(MultiBinding), "Bindings"),
			(typeof(MultiPage<Page>), "Children"),
			(typeof(MultiTrigger), "Setters"),
			(typeof(On), "Value"),
			(typeof(OnPlatform<object>), "Platforms"),
			(typeof(RadioButton), "Content"),
			(typeof(RefreshView), "Content"),
			(typeof(ScrollView), "Content"),
			(typeof(Setter), "Value"),
			(typeof(Shapes.GeometryGroup), "Children"),
			(typeof(Shapes.Path), "Data"),
			(typeof(Shapes.PathFigure), "Segments"),
			(typeof(Shapes.PathGeometry), "Figures"),
			(typeof(Shapes.Polygon), "Points"),
			(typeof(Shapes.Polyline), "Points"),
			(typeof(Shapes.TransformGroup), "Children"),
			(typeof(Shell), "Items"),
			(typeof(ShellContent), "Content"),
			(typeof(ShellItem), "Items"),
			(typeof(ShellSection), "Items"),
			(typeof(Span), "Text"),
			(typeof(Style), "Setters"),
			(typeof(SwipeItemView), "Content"),
			(typeof(SwipeView), "Content"),
			(typeof(TabbedPage), "Children"),
			(typeof(TableView), "Root"),
			(typeof(Trigger), "Setters"),
			(typeof(VerticalStackLayout), "Children"),
			(typeof(ViewCell), "View"),
			(typeof(VisualState), "Setters"),
			(typeof(VisualStateGroup), "States"),
			(typeof(Window), "Page")
		};

		static bool IsContentPopertyAttributeValid(Type type, string propertyName)
		{
			var attributes = Attribute.GetCustomAttributes(type);
			var attribute = attributes.FirstOrDefault(e => e is ContentPropertyAttribute) as ContentPropertyAttribute;
			return attribute != null ? attribute.Name.Equals(propertyName, StringComparison.Ordinal) : false;
		}

		[Fact]
		public void CheckContentPropertyAttributes()
		{
			foreach (var classWithContentProp in classesWithContentPropertyNames)
			{
				var classHasValidContentProperty = IsContentPopertyAttributeValid(classWithContentProp.ClassType, classWithContentProp.ContentPropertyName);
				Assert.True(classHasValidContentProperty, $"The {classWithContentProp.ClassType.FullName} class has an invalid ContentPropertyAttribute. It must be [ContentProperty({classWithContentProp.ContentPropertyName})]");
			}
		}

		[Fact]
		public void ClassesWithContentPropertyImplementIEnumerable()
		{
			foreach (var classWithContentProp in classesWithContentPropertyNames)
			{
				Assert.True(typeof(IEnumerable).IsAssignableFrom(classWithContentProp.ClassType), $"Class {classWithContentProp.ClassType.FullName} must implement the IEnumerable interface");
			}
		}
	}
}