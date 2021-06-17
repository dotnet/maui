using System;
using System.Diagnostics;
using Maui.Controls.Sample.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public class MyApp : Application
	{
		public MyApp(IServiceProvider services, ITextService textService, IImageSourceServiceConfiguration imageConfig)
		{
			Services = services;
			Debug.WriteLine($"The injected text service had a message: '{textService.GetText()}'");
			imageConfig.SetImageDirectory("Assets");
		}

		public IServiceProvider Services { get; }

		protected override IWindow CreateWindow(IActivationState activationState)
		{
			return Services.GetRequiredService<IWindow>();
		}
	}
}