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
			new SectionModel(typeof(GridLayoutPage), "GridLayout",
				"The GridLayout is a layout that organizes its children into rows and columns, which can have proportional or absolute sizes."),

			new SectionModel(typeof(HorizontalStackLayoutPage), "HorizontalStackLayout",
				"A HorizontalStackLayout organizes child views in a one-dimensional horizontal stack."),

			new SectionModel(typeof(VerticalStackLayoutPage), "VerticalStackLayout",
				"A VerticalStackLayout organizes child views in a one-dimensional vertical stack."),
		};
	}
}