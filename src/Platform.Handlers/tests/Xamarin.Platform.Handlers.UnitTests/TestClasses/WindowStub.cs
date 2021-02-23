using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Platform.Handlers.UnitTests.TestClasses
{
	class WindowStub : IWindow
	{
		public IPage Page { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public IMauiContext MauiContext { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
	}
}
