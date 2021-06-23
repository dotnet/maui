using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Animations;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	class HandlersContextStub : IMauiContext
	{
		IServiceProvider _services;
		IMauiHandlersServiceProvider _mauiHandlersServiceProvider;
		IAnimationManager _animationManager;
		public HandlersContextStub(IServiceProvider services)
		{
			_services = services;
			_mauiHandlersServiceProvider = Services.GetService<IMauiHandlersServiceProvider>() ?? throw new NullReferenceException(nameof(IMauiServiceProvider));
			_animationManager = Services.GetService<IAnimationManager>() ?? new AnimationManager { Ticker = new MockTicker(), SpeedModifier = 10 };
		}

		public IServiceProvider Services => _services;

		public IMauiHandlersServiceProvider Handlers => _mauiHandlersServiceProvider;

		public IAnimationManager AnimationManager => _animationManager;
	}
}