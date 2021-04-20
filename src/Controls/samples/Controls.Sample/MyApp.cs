using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Maui.Controls.Sample.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public class MyApp : IApplication
	{
		public MyApp(IServiceProvider services, ITextService textService, IList<IWindow> windows)
		{
			Services = services;
			Windows = windows.ToList();

			Debug.WriteLine($"The injected text service had a message: '{textService.GetText()}'");
			Device.StartTimer(TimeSpan.FromSeconds(3), () => {
				var window = Windows.FirstOrDefault();
				Debug.WriteLine($"The first window is: '{window.Title}'");
				return false;
			});
		}

		public IServiceProvider Services { get; }

		public IReadOnlyList<IWindow> Windows { get; }

		public IWindow CreateWindow(IActivationState activationState)
		{
			Microsoft.Maui.Controls.Compatibility.Forms.Init(activationState);

			return Services.GetRequiredService<IWindow>();
		}
	}
}