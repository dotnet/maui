using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class ApplicationStub : Application
	{
		Window _window;

		public ApplicationStub() : base(false)
		{
		}

		public void SetWindow(Window window) => _window = window;

		protected override Window CreateWindow(IActivationState activationState)
		{
			return _window ?? base.CreateWindow(activationState);
			;
		}
	}
}
