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

		public ApplicationStub() : this(Application.Current)
		{
		}

		private ApplicationStub(Application current) : base() //: base(false)
		{
			// Base only sets ApplicationCurrent is "true" is passed (the default)
			// It sees easier to revert this here instead of authoring Reflection to call the internal base constructor.
			Application.Current = current;
		}

		public void SetWindow(Window window) => _window = window;

		protected override Window CreateWindow(IActivationState activationState)
		{
			return _window ?? base.CreateWindow(activationState);
			;
		}
	}
}
