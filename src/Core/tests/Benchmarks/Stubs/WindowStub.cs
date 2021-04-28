using System;

namespace Microsoft.Maui.Handlers.Benchmarks
{
	public class WindowStub : StubBase, IWindow
	{
		public IView View { get; set; }
	}
}