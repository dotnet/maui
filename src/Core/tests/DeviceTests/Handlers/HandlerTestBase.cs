using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class HandlerTestBase<THandler, TStub> : TestBase, IDisposable
		where THandler : IViewHandler
		where TStub : StubBase, IView, new()
	{
		IApplication _app;
		IAppHost _host;
		IMauiContext _context;

		public HandlerTestBase()
		{
			var appBuilder = AppHost
				.CreateDefaultBuilder()
				.ConfigureImageSources((ctx, services) =>
				{
					services.AddService<ICountedImageSourceStub, CountedImageSourceServiceStub>();
				})
				.ConfigureFonts((ctx, fonts) =>
				{
					fonts.AddFont("dokdo_regular.ttf", "Dokdo");
					fonts.AddFont("LobsterTwo-Regular.ttf", "Lobster Two");
					fonts.AddFont("LobsterTwo-Bold.ttf", "Lobster Two Bold");
					fonts.AddFont("LobsterTwo-Italic.ttf", "Lobster Two Italic");
					fonts.AddFont("LobsterTwo-BoldItalic.ttf", "Lobster Two BoldItalic");
				});

			_host = appBuilder.Build();

			_app = new ApplicationStub();

			_context = new ContextStub(_host.Services);
		}

		public void Dispose()
		{
			_host.Dispose();
			_host = null;
			_app = null;
			_context = null;
		}

		public IApplication App => _app;

		public IMauiContext MauiContext => _context;

		protected THandler CreateHandler(IView view) =>
			CreateHandler<THandler>(view);

		protected TCustomHandler CreateHandler<TCustomHandler>(IView view)
			where TCustomHandler : THandler
		{
			var handler = Activator.CreateInstance<TCustomHandler>();
			handler.SetMauiContext(MauiContext);

			handler.SetVirtualView(view);
			view.Handler = handler;

			view.Arrange(new Rectangle(0, 0, view.Width, view.Height));
			handler.NativeArrange(view.Frame);

			return handler;
		}

		protected async Task<THandler> CreateHandlerAsync(IView view)
		{
			return await InvokeOnMainThreadAsync(() =>
			{
				return CreateHandler(view);
			});
		}

		protected Task<TValue> GetValueAsync<TValue>(IView view, Func<THandler, TValue> func)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler(view);
				return func(handler);
			});
		}

		protected Task SetValueAsync<TValue>(IView view, TValue value, Action<THandler, TValue> func)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler(view);
				func(handler, value);
			});
		}

		async protected Task ValidatePropertyInitValue<TValue>(
			IView view,
			Func<TValue> GetValue,
			Func<THandler, TValue> GetNativeValue,
			TValue expectedValue)
		{
			var values = await GetValueAsync(view, (handler) =>
			{
				return new
				{
					ViewValue = GetValue(),
					NativeViewValue = GetNativeValue(handler)
				};
			});

			Assert.Equal(expectedValue, values.ViewValue);
			Assert.Equal(expectedValue, values.NativeViewValue);
		}

		async protected Task ValidatePropertyUpdatesValue<TValue>(
			IView view,
			string property,
			Func<THandler, TValue> GetNativeValue,
			TValue expectedSetValue,
			TValue expectedUnsetValue)
		{
			var propInfo = view.GetType().GetProperty(property);

			// set initial values

			propInfo.SetValue(view, expectedSetValue);

			var (handler, viewVal, nativeVal) = await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler(view);
				return (handler, (TValue)propInfo.GetValue(view), GetNativeValue(handler));
			});

			Assert.Equal(expectedSetValue, viewVal);
			Assert.Equal(expectedSetValue, nativeVal);

			// confirm can update

			(viewVal, nativeVal) = await InvokeOnMainThreadAsync(() =>
			{
				propInfo.SetValue(view, expectedUnsetValue);
				handler.UpdateValue(property);

				return ((TValue)propInfo.GetValue(view), GetNativeValue(handler));
			});

			Assert.Equal(expectedUnsetValue, viewVal);
			Assert.Equal(expectedUnsetValue, nativeVal);

			// confirm can revert

			(viewVal, nativeVal) = await InvokeOnMainThreadAsync(() =>
			{
				propInfo.SetValue(view, expectedSetValue);
				handler.UpdateValue(property);

				return ((TValue)propInfo.GetValue(view), GetNativeValue(handler));
			});

			Assert.Equal(expectedSetValue, viewVal);
			Assert.Equal(expectedSetValue, nativeVal);
		}

		async protected Task ValidateUnrelatedPropertyUnaffected<TValue>(
			IView view,
			Func<THandler, TValue> GetNativeValue,
			string property,
			Action SetUnrelatedProperty)
		{
			// get initial values

			var (handler, initialNativeVal) = await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler(view);
				return (handler, GetNativeValue(handler));
			});

			// run update

			var newNativeVal = await InvokeOnMainThreadAsync(() =>
			{
				SetUnrelatedProperty();
				handler.UpdateValue(property);

				return GetNativeValue(handler);
			});

			// ensure unchanged

			Assert.Equal(initialNativeVal, newNativeVal);
		}
	}
}