using System;

namespace Microsoft.Maui.Handlers.Benchmarks
{
	public class WindowStub : IWindow
	{
		public IMauiContext MauiContext { get; set; }
		public IPage Page { get; set; }
	}
}