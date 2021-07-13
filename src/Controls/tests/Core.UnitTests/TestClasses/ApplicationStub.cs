using System.Collections.Generic;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	class ApplicationStub : IApplication
	{
		readonly List<IWindow> _windows = new List<IWindow>();

		public IReadOnlyList<IWindow> Windows => _windows.AsReadOnly();

		public string Property { get; set; } = "Default";

		public IWindow CreateWindow(IActivationState activationState)
		{
			throw new System.NotImplementedException();
		}

		public void ThemeChanged() { }
	}
}