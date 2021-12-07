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

			RequestedThemeChanged += (sender, args) =>
			{
				// Respond to the theme change
				Debug.WriteLine($"Requested theme changed: {args.RequestedTheme}");
			};
		}

		// Must not use MainPage for multi-window
		protected override Window CreateWindow(IActivationState activationState)
		{
			var window = new Window(Services.GetRequiredService<Page>());
			window.Title = ".NET MAUI Samples Gallery";
			return window;
		}

		public IServiceProvider Services { get; }
	}
}