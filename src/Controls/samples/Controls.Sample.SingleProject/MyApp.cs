using Microsoft.Maui;
using System.Collections.Generic;

namespace Maui.Controls.Sample.SingleProject
{
	public class MyApp : IApplication
	{
		List<IWindow> _windows = new List<IWindow>();
		public IReadOnlyList<IWindow> Windows => _windows.AsReadOnly();

		public IWindow CreateWindow(IActivationState activationState)
		{
			Microsoft.Maui.Controls.Compatibility.Forms.Init(activationState);
			var window =  Services.GetRequiredService<IWindow>();
			_windows.Add(window);
			return new MainWindow();
		}
	}
}