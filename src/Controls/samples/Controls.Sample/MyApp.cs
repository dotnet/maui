using System;
using System.Collections.Generic;
using System.Diagnostics;
using Maui.Controls.Sample.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;

namespace Maui.Controls.Sample
{
	public class MyApp : IApplication
	{
		public MyApp(IServiceProvider services, ITextService textService, List<IWindow> windows)
		{
			Services = services;

			Windows = windows;

			Debug.WriteLine($"The injected text service had a message: '{textService.GetText()}'");
		}

		public IServiceProvider Services { get; }

		public IReadOnlyList<IWindow> Windows { get; internal set; }

		public IWindow CreateWindow(IActivationState activationState)
		{
			Microsoft.Maui.Controls.Compatibility.Forms.Init(activationState);

			return Services.GetRequiredService<IWindow>();
		}
	}
}