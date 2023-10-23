using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.SimpleSampleApp
{
	public class SimpleSampleMauiApp : Application
	{
		public SimpleSampleMauiApp(IServiceProvider services, ITextService textService)
		{
			Services = services;
			MainPage = services.GetService<Page>();
			Debug.WriteLine($"The injected text service had a message: '{textService.GetText()}'");
		}

		public IServiceProvider Services { get; }

		public void ThemeChanged() { }
	}
}