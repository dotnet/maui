using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Animations;

namespace Microsoft.Maui.UnitTests
{
	class HandlersContextStub : IMauiContext
	{
		public HandlersContextStub(IServiceProvider services)
		{
			Services = services;
			Handlers = Services.GetRequiredService<IMauiHandlersServiceProvider>();
			AnimationManager = services.GetService<IAnimationManager>();
		}

		public IServiceProvider Services { get; }

		public IMauiHandlersServiceProvider Handlers { get; }

		public IAnimationManager AnimationManager { get; }
	}
}