using System;
using Maui.Controls.Sample.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Compatibility;

namespace Maui.Controls.Sample
{
	public class MyApp : MauiApp
	{
		public MyApp(ITextService textService)
		{
			Console.WriteLine($"The injected text service had a message: '{textService.GetText()}'");
		}

		public override IWindow CreateWindow(IActivationState state)
		{
			Forms.Init(state);
			return Services.GetRequiredService<IWindow>();
		}
	}
}