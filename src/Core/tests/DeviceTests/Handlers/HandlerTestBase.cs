using System;
using System.Threading.Tasks;
using Microsoft.Maui.Essentials;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Collection(TestCollections.Handlers)]
	public partial class HandlerTestBase<THandler> : TestBase
		where THandler : IViewHandler
	{
		readonly HandlerTestFixture _fixture;

		public HandlerTestBase(HandlerTestFixture fixture)
		{
			_fixture = fixture;
		}

		public IApp App => _fixture.App;

		public Task<T> InvokeOnMainThreadAsync<T>(Func<T> func) =>
			MainThread.InvokeOnMainThreadAsync(func);

		public Task InvokeOnMainThreadAsync(Action action) =>
			MainThread.InvokeOnMainThreadAsync(action);

		public Task InvokeOnMainThreadAsync(Func<Task> func) =>
			MainThread.InvokeOnMainThreadAsync(func);

		public Task<T> InvokeOnMainThreadAsync<T>(Func<Task<T>> func) =>
			MainThread.InvokeOnMainThreadAsync(func);

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
	}
}