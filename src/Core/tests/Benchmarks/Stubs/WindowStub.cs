using System;

namespace Microsoft.Maui.Handlers.Benchmarks
{
	public class WindowStub : StubBase, IWindow
	{
		public IView Content { get; set; }

		public string Title { get; set; }
	}
}