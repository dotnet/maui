using System;
using System.Diagnostics;
using Maui.Controls.Sample.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public partial class XamlApp : Application
	{
		public XamlApp(IServiceProvider services, ITextService textService)
		{
			InitializeComponent();

			Services = services;

			Debug.WriteLine($"The injected text service had a message: '{textService.GetText()}'");

			//MainPage = new Pages.AppShell();
			//MainPage = Services.GetRequiredService<Page>();
			MainPage = new TabbedPage()
			{
				Children =
				{
					Services.GetRequiredService<Page>(),
					new NavigationPage(new Pages.NavigationGallery()) { Title = "Semantics" }
				}
			};

			//MainPage = new NavigationPage(new TabbedPage()
			//{
			//	Children =
			//	{
			//		 new Pages.NavigationGallery(),
			//		 new Pages.NavigationGallery(),
			//	}
			//});
			//MainPage = new Pages.ImagePage();
			//Children =
			//{
			//	Services.GetRequiredService<Page>(),
			//	new NavigationPage(new Pages.NavigationGallery()) { Title = "Semantics" }
			//}
			//Children =
			//{
			//	new Pages.ButtonPage() { Title = "Buttons" },
			//	new Pages.SemanticsPage() { Title = "Semantics" }
			//}


			RequestedThemeChanged += (sender, args) =>
			{
				// Respond to the theme change
				Debug.WriteLine($"Requested theme changed: {args.RequestedTheme}");
			};
		}

		public IServiceProvider Services { get; }
	}
}
