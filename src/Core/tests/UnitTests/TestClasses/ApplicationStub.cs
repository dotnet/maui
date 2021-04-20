using System.Collections.Generic;

namespace Microsoft.Maui.UnitTests
{
	class ApplicationStub : IApplication
	{
		public string Property { get; set; } = "Default";

		public IReadOnlyList<IWindow> Windows => throw new System.NotImplementedException();

		public IWindow CreateWindow(IActivationState activationState)
		{
			throw new System.NotImplementedException();
		}
	}
}