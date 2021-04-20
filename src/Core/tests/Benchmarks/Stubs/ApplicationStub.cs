using System.Collections.Generic;

namespace Microsoft.Maui.Handlers.Benchmarks
{
	class ApplicationStub : IApplication
	{
		public IReadOnlyList<IWindow> Windows => throw new System.NotImplementedException();

		public IWindow CreateWindow(IActivationState state)
		{
			return new WindowStub();
		}
	}
}