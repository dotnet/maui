using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Animations;

namespace Microsoft.Maui.Handlers.Benchmarks
{
	class HandlersContextStub : IMauiContext
	{
		readonly IServiceProvider _services;
		readonly IMauiHandlersFactory _handlersServiceProvider;
		readonly IAnimationManager _animationManager;

		public HandlersContextStub(IServiceProvider services)
		{
			_services = services;
			_handlersServiceProvider = Services.GetRequiredService<IMauiHandlersFactory>();
			_animationManager = Services.GetRequiredService<IAnimationManager>();
		}

		public IServiceProvider Services => _services;

		public IMauiHandlersFactory Handlers => _handlersServiceProvider;

		public IAnimationManager AnimationManager => _animationManager;
	}
}
