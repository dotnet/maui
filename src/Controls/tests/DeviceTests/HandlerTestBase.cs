using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests
{
	public partial class HandlerTestBase : TestBase, IDisposable
	{
		MauiApp _mauiApp;

		public HandlerTestBase()
		{
			_mauiApp = MauiApp
				.CreateBuilder()
				.Build();

#if __ANDROID__
			MauiContext = new MauiContext(_mauiApp.Services, MauiProgram.CurrentContext);
#else
			MauiContext = new MauiContext(_mauiApp.Services);
#endif
		}

		public void Dispose()
		{
			((IDisposable)_mauiApp).Dispose();

			_mauiApp = null;
			App = null;
			MauiContext = null;
		}

		protected IApplication App { get; private set; }

		protected IMauiContext MauiContext { get; private set; }

		protected THandler CreateHandler<THandler>(IView view)
			where THandler : IViewHandler
		{
			var handler = Activator.CreateInstance<THandler>();
			handler.SetMauiContext(MauiContext);

			handler.SetVirtualView(view);
			view.Handler = handler;

			view.Arrange(new Rectangle(0, 0, view.Width, view.Height));
			handler.NativeArrange(view.Frame);

			return handler;
		}

		protected async Task<THandler> CreateHandlerAsync<THandler>(IView view) where THandler : IViewHandler =>
			await InvokeOnMainThreadAsync(() => CreateHandler<THandler>(view));
	}
}