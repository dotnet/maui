using System;
using Microsoft.Maui.DeviceTests.Stubs;

namespace Microsoft.Maui.DeviceTests
{
	public class ElementHandlerStub : ElementHandler<ElementStub, object>
	{
		public ElementHandlerStub() : base(ElementHandler.ElementMapper)
		{

		}

		protected override object CreatePlatformElement() => new Object();
	}
}
