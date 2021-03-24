using System;
using Maui.Controls.Sample.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Compatibility;

namespace Maui.Controls.Sample
{
	public class MyApp : IApplication
	{
		public MyApp(IServiceProvider services, ITextService textService)
		{
			Services = services;

			Console.WriteLine($"The injected text service had a message: '{textService.GetText()}'");
		}

		public IServiceProvider Services { get; }

		public IWindow CreateWindow(IActivationState activationState)
		{
			Forms.Init(activationState);

			return Services.GetRequiredService<IWindow>();
		}
	}
}