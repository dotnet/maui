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

			Debug.WriteLine($"The AccentColor color is {Resources["AccentColor"]}");
			Debug.WriteLine($"The injected text service had a message: '{textService.GetText()}'");

			MainPage = Services.GetRequiredService<Page>();
		}

		public IServiceProvider Services { get; }
	}
}
