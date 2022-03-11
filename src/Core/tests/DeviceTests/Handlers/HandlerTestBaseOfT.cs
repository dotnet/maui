using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class HandlerTestBase<THandler, TStub> : HandlerTestBase
		where THandler : IViewHandler, new()
		where TStub : StubBase, IView, new()
	{
		static readonly Random rnd = new Random();


		public static async Task<bool> Wait(Func<bool> exitCondition, int timeout = 1000)
		{
			while ((timeout -= 100) > 0)
			{
				if (!exitCondition.Invoke())
					await Task.Delay(rnd.Next(100, 200));
				else
					break;
			}

			return exitCondition.Invoke();
		}

		protected THandler CreateHandler(IView view, IMauiContext mauiContext = null) =>
			CreateHandler<THandler>(view, mauiContext);

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
			Func<THandler, TValue> GetPlatformValue,
			TValue expectedValue)
		{
			var values = await GetValueAsync(view, (handler) =>
			{
				return new
				{
					ViewValue = GetValue(),
					PlatformViewValue = GetPlatformValue(handler)
				};
			});

			Assert.Equal(expectedValue, values.ViewValue);
			Assert.Equal(expectedValue, values.PlatformViewValue);
		}

		async protected Task ValidatePropertyUpdatesValue<TValue>(
			IView view,
			string property,
			Func<THandler, TValue> GetPlatformValue,
			TValue expectedSetValue,
			TValue expectedUnsetValue)
		{
			var propInfo = view.GetType().GetProperty(property);

			// set initial values

			propInfo.SetValue(view, expectedSetValue);

			var (handler, viewVal, nativeVal) = await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler(view);
				return (handler, (TValue)propInfo.GetValue(view), GetPlatformValue(handler));
			});

			Assert.Equal(expectedSetValue, viewVal);
			Assert.Equal(expectedSetValue, nativeVal);

			// confirm can update

			(viewVal, nativeVal) = await InvokeOnMainThreadAsync(() =>
			{
				propInfo.SetValue(view, expectedUnsetValue);
				handler.UpdateValue(property);

				return ((TValue)propInfo.GetValue(view), GetPlatformValue(handler));
			});

			Assert.Equal(expectedUnsetValue, viewVal);
			Assert.Equal(expectedUnsetValue, nativeVal);

			// confirm can revert

			(viewVal, nativeVal) = await InvokeOnMainThreadAsync(() =>
			{
				propInfo.SetValue(view, expectedSetValue);
				handler.UpdateValue(property);

				return ((TValue)propInfo.GetValue(view), GetPlatformValue(handler));
			});

			Assert.Equal(expectedSetValue, viewVal);
			Assert.Equal(expectedSetValue, nativeVal);
		}

		async protected Task ValidateUnrelatedPropertyUnaffected<TValue>(
			IView view,
			Func<THandler, TValue> GetPlatformValue,
			string property,
			Action SetUnrelatedProperty)
		{
			// get initial values

			var (handler, initialNativeVal) = await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler(view);
				return (handler, GetPlatformValue(handler));
			});

			// run update

			var newNativeVal = await InvokeOnMainThreadAsync(() =>
			{
				SetUnrelatedProperty();
				handler.UpdateValue(property);

				return GetPlatformValue(handler);
			});

			// ensure unchanged

			Assert.Equal(initialNativeVal, newNativeVal);
		}
	}
}