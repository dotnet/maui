using System;
using System.Threading.Tasks;
using Microsoft.Maui.Animations;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	class AnimationReadyHandler : AnimationReadyHandler<BlockingTicker>
	{
		public AnimationReadyHandler()
			: base(new TestAnimationManager(new BlockingTicker()))
		{
		}
	}

	class AnimationReadyHandlerAsync : AnimationReadyHandler<AsyncTicker>
	{
		public AnimationReadyHandlerAsync()
			: base(new TestAnimationManager(new AsyncTicker()))
		{
		}
	}

	class AnimationReadyHandler<TTicker> : ViewHandler<IView, object>
		where TTicker : ITicker, new()
	{
		public AnimationReadyHandler(IAnimationManager animationManager)
			: base(new PropertyMapper<IView>())
		{
			SetMauiContext(new AnimationReadyMauiContext(animationManager));
		}

		public static AnimationReadyHandler<TTicker> Prepare<T>(params T[] views)
			where T : View
		{
			AnimationReadyHandler<TTicker> handler = null;

			var ticker = new TestAnimationManager(new TTicker());

			foreach (var view in views)
			{
				handler = new AnimationReadyHandler<TTicker>(ticker);
				view.Handler = handler;
			}

			return handler;
		}

		public static T Prepare<T>(T view, out AnimationReadyHandler<TTicker> handler)
			where T : View
		{
			handler = new AnimationReadyHandler<TTicker>(new TestAnimationManager(new TTicker()));

			view.Handler = handler;

			return view;
		}

		public static T Prepare<T>(T view)
			where T : View
		{
			view.Handler = new AnimationReadyHandler();

			return view;
		}

		protected override object CreatePlatformView() => new();

		public IAnimationManager AnimationManager => ((AnimationReadyMauiContext)MauiContext).AnimationManager;

		class AnimationReadyMauiContext : IMauiContext, IServiceProvider
		{
			readonly IAnimationManager _animationManager;

			public AnimationReadyMauiContext(IAnimationManager manager = null)
			{
				_animationManager = manager ?? new TestAnimationManager();
			}

			public IServiceProvider Services => this;

			public IMauiHandlersFactory Handlers => throw new NotImplementedException();

			public IAnimationManager AnimationManager => _animationManager;

			public object GetService(Type serviceType)
			{
				if (serviceType == typeof(IAnimationManager))
					return _animationManager;

				if (serviceType == typeof(IDispatcherProvider))
				{
					return DispatcherProvider.Current;
				}

				throw new NotSupportedException($"Attempting to get service type {serviceType}");
			}
		}
	}

	static class AnimationReadyWindowExtensions
	{
		public static async Task DisableTicker(this AnimationReadyHandler<AsyncTicker> handler)
		{
			await Task.Delay(32);

			((AsyncTicker)handler.AnimationManager.Ticker).SetEnabled(false);
		}

		public static async Task EnableTicker(this AnimationReadyHandler<AsyncTicker> handler)
		{
			await Task.Delay(32);

			((AsyncTicker)handler.AnimationManager.Ticker).SetEnabled(true);
		}
	}
}