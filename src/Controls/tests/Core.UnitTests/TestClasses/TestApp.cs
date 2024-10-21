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
		Window _window;

		public TestApp() : base(false)
		{

		}

		public TestApp(Window window) : base(false)
		{
			_window = window;
		}

		public Window CreateWindow() =>
			(Window)(this as IApplication).CreateWindow(null);

		protected override Window CreateWindow(IActivationState activationState)
		{
			return _window ?? new Window(_withPage ?? new ContentPage());
		}

		public Window CreateWindow(Page withPage)
		{
			_withPage = withPage;
			return (Window)(this as IApplication).CreateWindow(null);
		}
	}
}
