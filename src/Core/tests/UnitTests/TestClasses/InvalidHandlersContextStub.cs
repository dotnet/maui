using System;
using Microsoft.Maui.Animations;

namespace Microsoft.Maui.UnitTests
{
	class InvalidHandlersContextStub : IMauiContext
	{
		public InvalidHandlersContextStub()
		{
		}

		public IServiceProvider Services => null!;

		public IMauiHandlersServiceProvider Handlers => null!;

		public IAnimationManager AnimationManager => null!;
	}
}