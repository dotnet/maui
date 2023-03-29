using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class TestApp : Application
	{
		Page _withPage;
		TestWindow _window;

		public TestApp() : base(false)
		{

		}

		public TestApp(TestWindow window) : base(false)
		{
			_window = window;
		}

		public TestWindow CreateWindow() =>
			(TestWindow)(this as IApplication).CreateWindow(null);

		protected override Window CreateWindow(IActivationState activationState)
		{
			return _window ?? new TestWindow(_withPage ?? new ContentPage());
		}

		public TestWindow CreateWindow(Page withPage)
		{
			_withPage = withPage;
			return (TestWindow)(this as IApplication).CreateWindow(null);
		}
	}
}
