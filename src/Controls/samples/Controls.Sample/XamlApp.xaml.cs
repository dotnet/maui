using System;
using System.Diagnostics;
using System.IO;
using Maui.Controls.Sample.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Essentials;

namespace Maui.Controls.Sample
{
	public partial class XamlApp : Application
	{
		public XamlApp(IServiceProvider services, ITextService textService)
		{
			InitializeComponent();

			Services = services;

			Debug.WriteLine($"The injected text service had a message: '{textService.GetText()}'");

			RequestedThemeChanged += (sender, args) =>
			{
				// Respond to the theme change
				Debug.WriteLine($"Requested theme changed: {args.RequestedTheme}");
			};

			LoadAsset();
		}

		async void LoadAsset()
		{
			try
			{
				using var stream = await FileSystem.OpenAppPackageFileAsync("RawAsset.txt");
				using var reader = new StreamReader(stream);

				Debug.WriteLine($"The raw Maui asset contents: '{reader.ReadToEnd().Trim()}'");
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Error loading the raw Maui asset contents: {ex}");
			}
		}

		// Must not use MainPage for multi-window
		protected override Window CreateWindow(IActivationState activationState)
		{
			var window = new Window(Services.GetRequiredService<Page>());

			var menuBarItem = new MenuBarItem() { Text = "Top Item" };
			menuBarItem.Add(new MenuFlyoutItem() { Text = "First Child" });
			menuBarItem.Add(new MenuFlyoutSubItem()
			{
				new MenuFlyoutItem()
				{
					Text = "Flyout Item",
					IconImageSource = "dotnet_bot.png"
				},

				new MenuFlyoutItem()
				{
					Text = "Flyout Item 2",
					IconImageSource = "dotnet_bot.png"
				}
			});

			(menuBarItem[1] as MenuFlyoutSubItem).Text = "Second Child";

			var fileBarItem = new MenuBarItem { Text = "File" };
			fileBarItem.Add(new MenuFlyoutItem() { Text = " Extra File item" });

			window.MenuBar = new MenuBar()
			{
				fileBarItem,
				menuBarItem,
				new MenuBarItem() { Text = "Edit" },
				new MenuBarItem() { Text = "Open" },
				new MenuBarItem() { Text = "View" },
				new MenuBarItem() { Text = "Close" },				
			};

			

			window.Title = ".NET MAUI Samples Gallery";
			return window;
		}

		public IServiceProvider Services { get; }
	}
}
