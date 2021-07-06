using System;
using System.Threading.Tasks;
using Microsoft.Maui.Animations;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	class AnimationReadyWindow : AnimationReadyWindow<BlockingTicker>
	{
		public AnimationReadyWindow(params View[] views)
			: base(views)
		{
		}
	}

	class AnimationReadyWindowAsync : AnimationReadyWindow<AsyncTicker>
	{
		public AnimationReadyWindowAsync(params View[] views)
			: base(views)
		{
		}
	}

	class AnimationReadyWindow<TTicker> : Window
		where TTicker : ITicker, new()
	{
		public AnimationReadyWindow(params View[] views)
		{
			Page = new ContentPage();

			SetChildren(views);

			Handler = new AnimationReadyHandler(new TestAnimationManager(new TTicker()));
		}

		public void SetChildren(params View[] views)
		{
			var grid = new Grid();

			foreach (var view in views)
				grid.Add(view);

			((ContentPage)Page).Content = grid;
		}

		public T SetChild<T>(T view)
			where T : View
		{
			((ContentPage)Page).Content = view;

			return view;
		}

		public static T Prepare<T>(T view, out AnimationReadyWindow<TTicker> window)
			where T : View
		{
			window = new AnimationReadyWindow<TTicker>(view);

			return view;
		}

		public static T Prepare<T>(T view)
			where T : View
		{
			var window = new AnimationReadyWindow<TTicker>(view);

			return view;
		}

		class AnimationReadyHandler : WindowHandler
		{
			public AnimationReadyHandler(IAnimationManager animationManager)
			{
				SetMauiContext(new AnimationReadyMauiContext(animationManager));
			}

			protected override object CreateNativeElement() => new();
		}

		class AnimationReadyMauiContext : IMauiContext, IServiceProvider
		{
			readonly IAnimationManager _animationManager;

			public AnimationReadyMauiContext(IAnimationManager manager = null)
			{
				_animationManager = manager ?? new TestAnimationManager();
			}

			public IServiceProvider Services => this;

			public IMauiHandlersServiceProvider Handlers => throw new NotImplementedException();

			public object GetService(Type type) =>
				type == typeof(IAnimationManager)
					? _animationManager
					: throw new NotImplementedException();
		}
	}

	static class AnimationReadyWindowExtensions
	{
		public static async Task DisableTicker(this AnimationReadyWindow<AsyncTicker> window)
		{
			await Task.Delay(32);

			((AsyncTicker)window.AnimationManager.Ticker).SetEnabled(false);
		}

		public static async Task EnableTicker(this AnimationReadyWindow<AsyncTicker> window)
		{
			await Task.Delay(32);

			((AsyncTicker)window.AnimationManager.Ticker).SetEnabled(true);
		}
	}
}