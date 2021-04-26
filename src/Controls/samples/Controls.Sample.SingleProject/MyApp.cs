using Microsoft.Maui;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Maui.Controls.Sample.SingleProject
{
	public class MyApp : IApplication
	{
		static bool UseBlazor = false;

		List<IWindow> _windows = new List<IWindow>();
		public IReadOnlyList<IWindow> Windows => _windows.AsReadOnly();

		public IWindow CreateWindow(IActivationState activationState)
		{
			Microsoft.Maui.Controls.Compatibility.Forms.Init(activationState);

			var window = new Microsoft.Maui.Controls.Window(UseBlazor ? new BlazorPage() : new MainPage());

			_windows.Add(window);			
			return window;
		}
	}
}