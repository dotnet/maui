﻿using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public abstract partial class FocusHandlerTests<THandler, TStub, TLayoutStub> : HandlerTestBasement<THandler, TStub>
		where THandler : class, IViewHandler, new()
		where TStub : IStubBase, new()
		where TLayoutStub : IStubBase, ILayout, new()
	{
		[Fact]
		public async Task FocusAndIsFocusedIsWorking()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handler =>
				{
					handler.AddHandler<TLayoutStub, LayoutHandler>();
					handler.AddHandler<TStub, THandler>();
				});
			});

			// create layout with 2 elements
			TStub inputControl1;
			TStub inputControl2;
			var layout = new TLayoutStub
			{
				(inputControl1 = new TStub { Width = 100, Height = 50 }),
				(inputControl2 = new TStub { Width = 100, Height = 50 })
			};
			layout.Width = 100;
			layout.Height = 150;

			await AttachAndRun<LayoutHandler>(layout, async (contentViewHandler) =>
			{
				var platform1 = inputControl1.ToPlatform();
				var platform2 = inputControl2.ToPlatform();

				// focus the first control
				var result1 = inputControl1.Handler.InvokeWithResult(nameof(IView.Focus), new FocusRequest());
				Assert.True(result1);

				// assert
				await inputControl1.WaitForFocused();
				Assert.True(inputControl1.IsFocused);
				Assert.False(inputControl2.IsFocused);

				// focus the second control
				var result2 = inputControl2.Handler.InvokeWithResult(nameof(IView.Focus), new FocusRequest());
				Assert.True(result2);

				// assert
				await inputControl2.WaitForFocused();
				Assert.False(inputControl1.IsFocused);
				Assert.True(inputControl2.IsFocused);
			});
		}

		[Fact]
		public async Task UnfocusAndIsFocusedIsWorking()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handler =>
				{
					handler.AddHandler<TLayoutStub, LayoutHandler>();
					handler.AddHandler<TStub, THandler>();
				});
			});

			// create layout with 2 elements
			TStub inputControl1;
			TStub inputControl2;
			var layout = new TLayoutStub
			{
				(inputControl1 = new TStub { Width = 100, Height = 50 }),
				(inputControl2 = new TStub { Width = 100, Height = 50 })
			};
			layout.Width = 100;
			layout.Height = 150;

			await AttachAndRun<LayoutHandler>(layout, async (contentViewHandler) =>
			{
				var platform1 = inputControl1.ToPlatform();
				var platform2 = inputControl2.ToPlatform();

				// focus the first control
				var result1 = inputControl1.Handler.InvokeWithResult(nameof(IView.Focus), new FocusRequest());
				Assert.True(result1);

				// assert
				await inputControl1.WaitForFocused();
				Assert.True(inputControl1.IsFocused);
				Assert.False(inputControl2.IsFocused);

				// UNfocus the first control (revert the focus)
				inputControl1.Handler.Invoke(nameof(IView.Unfocus));

				if (OperatingSystem.IsAndroid() &&
					!OperatingSystem.IsAndroidVersionAtLeast(28))
				{
					// After API 28 Android is able to unfocus all controls.
					// Before API 28 Android is not able to unfocus all controls.
					// It will always keep something focused.
					// Because inputControl1 is the first control in the layout
					// that's the control that will retain focus.
					return;
				}

				// assert
				await inputControl1.WaitForUnFocused();
				Assert.False(inputControl1.IsFocused);


				// Something always has to be focused in windows
				// So if you unfocus one control it'll just focus the other one on the screen
				if (!OperatingSystem.IsWindows())
				{
					await Task.Yield();
					Assert.False(inputControl2.IsFocused);
				}
			});
		}
	}
}