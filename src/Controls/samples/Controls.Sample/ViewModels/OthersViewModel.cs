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
			new SectionModel(typeof(StyleSheetsPage), "StyleSheets",
				"Demonstrates the usage of CSS in XAML.")
		};
	}
}