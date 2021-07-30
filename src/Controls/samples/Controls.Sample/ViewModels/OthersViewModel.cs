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
#if NET6_0_OR_GREATER	
			new SectionModel(typeof(BlazorPage), "BlazorWebView",
				"The BlazorWebView control allow to easily embed Blazor content with native UI."),		
#endif
			new SectionModel(typeof(GraphicsViewPage), "GraphicsView",
				"Allow to draw directly in a Canvas. You can combine a canvas and native Views on the same page."),
			new SectionModel(typeof(StyleSheetsPage), "StyleSheets",
				"Demonstrates the usage of CSS in XAML.")
		};
	}
}