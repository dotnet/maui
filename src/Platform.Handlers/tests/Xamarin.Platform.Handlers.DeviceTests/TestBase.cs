using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Platform.Handlers.DeviceTests.Stubs;

namespace Xamarin.Platform.Handlers.DeviceTests
{
	public partial class TestBase
	{
		static TestBase()
		{
			Xamarin.Platform.Registrar.Handlers.Register<SliderStub, SliderHandler>();
		}
	}
}
