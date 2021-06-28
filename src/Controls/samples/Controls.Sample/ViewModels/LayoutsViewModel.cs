using System.Collections.Generic;
using Maui.Controls.Sample.Models;
using Maui.Controls.Sample.Pages;
using Maui.Controls.Sample.ViewModels.Base;

namespace Maui.Controls.Sample.ViewModels
{
	public class LayoutsViewModel : BaseGalleryViewModel
	{
		protected override IEnumerable<SectionModel> CreateItems() => new[]
		{
			new SectionModel(typeof(AbsoluteLayoutPage), "AbsoluteLayout",
				"An AbsoluteLayout is used to position and size children using explicit values. The position is specified by the upper-left corner of the child relative to the upper-left corner of the AbsoluteLayout, in device-independent units."),

			new SectionModel(typeof(FlexLayoutPage), "FlexLayout",
				"FlexLayout is also capable of wrapping its children if there are too many to fit in a single row or column, and also has many options for orientation, alignment, and adapting to various screen sizes."),

			new SectionModel(typeof(GridPage), "Grid",
				"The Grid is a layout that organizes its children into rows and columns, which can have proportional or absolute sizes."),

			new SectionModel(typeof(RelativeLayoutPage), "RelativeLayout",
				"A RelativeLayout is used to position and size children relative to properties of the layout or sibling elements. This allows UIs to be created that scale proportionally across device sizes."),

			new SectionModel(typeof(ScrollViewPage), "ScrollView",
				"ScrollView is capable of scrolling its contents."),

			new SectionModel(typeof(StackLayoutPage), "StackLayout",
				"A StackLayout organizes child views in a one-dimensional horizontal or vertical stack."),

			new SectionModel(typeof(HorizontalStackLayoutPage), "HorizontalStackLayout",
				"A HorizontalStackLayout organizes child views in a one-dimensional horizontal stack."),

			new SectionModel(typeof(VerticalStackLayoutPage), "VerticalStackLayout",
				"A VerticalStackLayout organizes child views in a one-dimensional vertical stack."),
		};
	}
}