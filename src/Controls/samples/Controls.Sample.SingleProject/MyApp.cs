using Microsoft.Maui;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Maui.Controls.Sample.SingleProject
{
	public class MyApp : IApplication
	{
		List<IWindow> _windows = new List<IWindow>();
		public IReadOnlyList<IWindow> Windows => _windows.AsReadOnly();

		public IWindow CreateWindow(IActivationState activationState)
		{
			var window = new MainWindow();
			_windows.Add(window);
			return window;
		}
	}
}