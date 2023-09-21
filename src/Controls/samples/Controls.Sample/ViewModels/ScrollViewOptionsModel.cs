using System.Collections.Generic;
using Maui.Controls.Sample.Models;
using Maui.Controls.Sample.Pages.ScrollViewPages;
using Maui.Controls.Sample.ViewModels.Base;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.ViewModels
{
	public class ScrollViewOptionsModel : BaseGalleryViewModel
	{
		protected override IEnumerable<SectionModel> CreateItems() => new[]
		{
			new SectionModel(typeof(ScrollToFromConstructorPage), "Scroll To from constructor",
				"ScrollView is capable of scrolling its contents invoking the scroll from the Page constructor",
				new ScrollToFromConstructorPage()),

			new SectionModel(typeof(ScrollToEndPage), "Scroll To End",
				"ScrollView is capable of scrolling its contents."),

			new SectionModel(typeof(ScrollViewTemplatePage), "Default Template",
				"Default Template", new ScrollViewTemplatePageModel()),

			new SectionModel(typeof(ScrollViewTemplatePage), "Align Start",
				"Vertical Align Start", new ScrollViewTemplatePageModel{ VerticalAlignment = LayoutOptions.Start }),

			new SectionModel(typeof(ScrollViewTemplatePage), "Align End",
				"Vertical Align End", new ScrollViewTemplatePageModel{ VerticalAlignment = LayoutOptions.End }),

			new SectionModel(typeof(ScrollViewTemplatePage), "Align Fill",
				"Vertical Align Fill", new ScrollViewTemplatePageModel{ VerticalAlignment = LayoutOptions.Fill }),

			new SectionModel(typeof(ScrollViewTemplatePage), "Large Item Spacing",
				"Default Template with Large Item Spacing", new ScrollViewTemplatePageModel{ Spacing = 200 }),

			new SectionModel(typeof(ScrollViewTemplatePage), "Large Item Spacing, ScrollView Padding",
				"Default Template with Large Item Spacing and ScrollView Padding",
				new ScrollViewTemplatePageModel{ Spacing = 200, ScrollViewPadding = new Thickness(25),
					ContentBackground = Colors.LightBlue, VerticalAlignment = LayoutOptions.Fill }),

			new SectionModel(typeof(ScrollViewOrientationPage), "Orientation",
				"Lock the orientation of your ScrollView",
				new ScrollViewTemplatePageModel{ VerticalAlignment = LayoutOptions.Fill }),
		};
	}
}