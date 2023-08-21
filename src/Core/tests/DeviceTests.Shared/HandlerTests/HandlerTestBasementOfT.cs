using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.DeviceTests
{
	public partial class HandlerTestBasement<THandler, TStub> : HandlerTestBasement
		where THandler : class, IViewHandler, new()
		where TStub : IStubBase, IView, new()
	{
		public static Task<bool> Wait(Func<bool> exitCondition, int timeout = 1000) =>
			AssertionExtensions.Wait(exitCondition, timeout);

		protected THandler CreateHandler(IView view, IMauiContext mauiContext = null) =>
			CreateHandler<THandler>(view, mauiContext);

		public Task AttachAndRun(IView view, Action<THandler> action) =>
				AttachAndRun<bool>(view, (handler) =>
				{
					action(handler);
					return Task.FromResult(true);
				});

		public Task AttachAndRun(IView view, Func<THandler, Task> action) =>
				AttachAndRun<bool>(view, async (handler) =>
				{
					await action(handler);
					return true;
				});

		public Task<T> AttachAndRun<T>(IView view, Func<THandler, T> action)
		{
			Func<THandler, Task<T>> boop = (handler) =>
			{
				return Task.FromResult(action.Invoke(handler));
			};

			return AttachAndRun<T>(view, boop);
		}

		public Task<T> AttachAndRun<T>(IView view, Func<THandler, Task<T>> action)
		{
			return view.AttachAndRun<T, IPlatformViewHandler>((handler) =>
			{
				return action.Invoke((THandler)handler);
			}, MauiContext, async (view) => (IPlatformViewHandler)(await CreateHandlerAsync(view)));
		}

		protected Task<THandler> CreateHandlerAsync(IView view)
		{
			return InvokeOnMainThreadAsync(() =>
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

		protected Task<TValue> GetValueAsync<TValue>(IView view, Func<THandler, Task<TValue>> func)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler(view);
				return func(handler);
			});
		}

		protected Task<TValue> GetValueAsync<TValue, TCustomHandler>(IView view, Func<TCustomHandler, TValue> func)
			where TCustomHandler : IElementHandler, new()
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<TCustomHandler>(view);
				return func(handler);
			});
		}

		protected Task<TValue> GetValueAsync<TValue, TCustomHandler>(IView view, Func<TCustomHandler, Task<TValue>> func)
			where TCustomHandler : IElementHandler, new()
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<TCustomHandler>(view);
				return func(handler);
			});
		}

		protected Task SetValueAsync<TValue>(IView view, TValue value, Action<THandler, TValue> func)
		{
			return SetValueAsync<TValue, THandler>(view, value, func);
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

		async protected Task ValidatePropertyInitValue<TValue, TCustomHandler>(
			IView view,
			Func<TValue> GetValue,
			Func<TCustomHandler, TValue> GetPlatformValue,
			TValue expectedValue)
			where TCustomHandler : IElementHandler, new()
		{
			var values = await GetValueAsync(view, (TCustomHandler handler) =>
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

		async protected Task ValidatePropertyInitValue<TValue>(
			IView view,
			Func<TValue> GetValue,
			Func<THandler, TValue> GetPlatformValue,
			TValue expectedValue,
			TValue expectedPlatformValue)
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
			Assert.Equal(expectedPlatformValue, values.PlatformViewValue);
		}

		async protected Task ValidatePropertyInitValue<TValue, TCustomHandler>(
			IView view,
			Func<TValue> GetValue,
			Func<TCustomHandler, TValue> GetPlatformValue,
			TValue expectedValue,
			TValue expectedPlatformValue)
			where TCustomHandler : IElementHandler, new()
		{
			var values = await GetValueAsync(view, (TCustomHandler handler) =>
			{
				return new
				{
					ViewValue = GetValue(),
					PlatformViewValue = GetPlatformValue(handler)
				};
			});

			Assert.Equal(expectedValue, values.ViewValue);
			Assert.Equal(expectedPlatformValue, values.PlatformViewValue);
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

			await ValidatePropertyUpdatesAfterInitValue(handler, property, GetPlatformValue, expectedSetValue, expectedUnsetValue);
		}

		async protected Task ValidatePropertyUpdatesAfterInitValue<TValue>(
			THandler handler,
			string property,
			Func<THandler, TValue> GetPlatformValue,
			TValue expectedSetValue,
			TValue expectedUnsetValue)
		{
			var view = handler.VirtualView;
			var propInfo = handler.VirtualView.GetType().GetProperty(property);

			// confirm can update

			var (viewVal, nativeVal) = await InvokeOnMainThreadAsync(() =>
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

		protected Task ValidateHasColor(IView view, Color color, Action action = null, string updatePropertyValue = null, double? tolerance = null) =>
			ValidateHasColor(view, color, typeof(THandler), action, updatePropertyValue, tolerance: tolerance);

		protected void MockAccessibilityExpectations(TStub view)
		{
#if IOS || MACCATALYST
			var mapperOverride = new PropertyMapper<TStub, THandler>();
			view.PropertyMapperOverrides = mapperOverride;

			mapperOverride
				.ModifyMapping(nameof(IView.Semantics), (handler, view, _) =>
				{
					(handler.PlatformView as UIKit.UIView)?.SetupAccessibilityExpectationIfVoiceOverIsOff();
					mapperOverride.Chained[0]!.UpdateProperty(handler, view, nameof(IView.Semantics));
				});
#endif
		}

		protected void AssertWithinTolerance(double expected, double actual, double tolerance = 0.2, string message = "Value was not within tolerance.")
		{
			var diff = System.Math.Abs(expected - actual);
			if (diff > tolerance)
			{
				throw new XunitException($"{message} Expected: {expected}; Actual: {actual}; Tolerance {tolerance}");
			}
		}

		protected void AssertWithinTolerance(Graphics.Size expected, Graphics.Size actual, double tolerance = 0.2)
		{
			AssertWithinTolerance(expected.Height, actual.Height, tolerance, "Height was not within tolerance.");
			AssertWithinTolerance(expected.Width, actual.Width, tolerance, "Width was not within tolerance.");
		}
	}
}