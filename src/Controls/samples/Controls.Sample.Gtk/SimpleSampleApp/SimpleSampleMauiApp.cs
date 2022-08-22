using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;

namespace Maui.SimpleSampleApp
{

	public class SimpleSampleMauiApp : IApplication
	{

		public SimpleSampleMauiApp(IServiceProvider services, ITextService textService)
		{
			Services = services;

			Debug.WriteLine($"The injected text service had a message: '{textService.GetText()}'");
		}

		readonly List<IWindow> _windows = new();

		public IReadOnlyList<IWindow> Windows => _windows.AsReadOnly();

		public IServiceProvider Services { get; }

		public IWindow CreateWindow(IActivationState activationState)
		{
			Microsoft.Maui.Controls.Compatibility.Forms.Init(activationState);

			var window = Services.GetRequiredService<IWindow>();

			_windows.Add(window);

			return window;
		}

		public void ThemeChanged()
		{
		}

	}

}