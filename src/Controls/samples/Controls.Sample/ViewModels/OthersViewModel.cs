using System.Collections.Generic;
using Maui.Controls.Sample.Models;
using Maui.Controls.Sample.Pages;
using Maui.Controls.Sample.ViewModels.Base;

namespace Maui.Controls.Sample.ViewModels
{
	public class OthersViewModel : BaseGalleryViewModel
	{
		protected override IEnumerable<SectionModel> CreateItems() => new[]
		{
			new SectionModel(typeof(GraphicsViewPage), "GraphicsView",
				"Allow to draw directly in a Canvas. You can combine a canvas and native Views on the same page."),

			new SectionModel(typeof(LargeTitlesPageiOS), "Large Titles - iOS",
				"This iOS platform-specific is used to display the page title as a large title on the navigation bar of a NavigationPage, for devices that use iOS 11 or greater."),

			new SectionModel(typeof(StyleSheetsPage), "StyleSheets",
				"Demonstrates the usage of CSS in XAML."),

			new SectionModel(typeof(TwoPaneViewPage), "Foldable",
				"Demonstrates the usage of TwoPaneView and hinge sensor."),

			new SectionModel(typeof(RenderViewPage), "Render Views",
				"Demonstrates rendering views as images."),

			new SectionModel(typeof(HitTestingPage), "Hit Testing",
				"Demonstrates VisualTreeElementExtensions hit testing methods"),
		};
	}
}