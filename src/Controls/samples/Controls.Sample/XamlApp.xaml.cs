using System;
using System.Diagnostics;
using System.IO;
using Maui.Controls.Sample.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace Maui.Controls.Sample
{
	public partial class XamlApp : Application
	{
		public XamlApp(IServiceProvider services, ITextService textService)
		{
			InitializeComponent();

			Services = services;

			Debug.WriteLine($"The injected text service had a message: '{textService.GetText()}'");

			var requested = services.GetRequiredService<ITextService>();
			Debug.WriteLine($"The requested text service had a message: '{requested.GetText()}'");

			Debug.WriteLine($"Current app theme: {RequestedTheme}");

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
		protected override Window CreateWindow(IActivationState? activationState)
		{
			var services = activationState!.Context.Services;
			var window = new MauiWindow(services.GetRequiredService<Page>())
			{
				Title = ".NET MAUI Samples Gallery"
			};

			return window;
		}

		public IServiceProvider Services { get; }
	}
}
