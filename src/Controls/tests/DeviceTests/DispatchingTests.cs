﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.TestUtils.DeviceTests.Runners;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Dispatcher)]
	public class DispatchingTests : ControlsHandlerTestBase
	{
		[Fact(
#if MACCATALYST
			Skip = "Fails on Mac Catalyst, fixme"
#endif
			)]
		public async Task DispatchFromBackgroundThread()
		{
			bool dispatched = false;
			await Task.Run(async () =>
			{
				var app = await InvokeOnMainThreadAsync(() => new Application(false));
				var handler = new ApplicationHandlerStub();
				handler.SetMauiContext(MauiContext);
				app.Handler = handler;

				await app
					.FindDispatcher()
					.DispatchAsync(async () =>
				{
					await Task.Delay(0);
					dispatched = true;
				});
			});

			await Task.Delay(500);
			Assert.True(dispatched);
		}

		[Fact]
		public async Task FindDispatcher_WithBindableObject_ReturnsValidDispatcher()
		{
			var button = await InvokeOnMainThreadAsync(() => new Button { Text = "Test" });
			var handler = new ButtonHandlerStub();
			handler.SetMauiContext(MauiContext);
			button.Handler = handler;

			var dispatcher = button.FindDispatcher();

			Assert.NotNull(dispatcher);
		}

		[Fact]
		public async Task DispatchIfRequired_OnMainThread_ExecutesSynchronously()
		{
			bool executed = false;
			var button = await InvokeOnMainThreadAsync(() => new Button { Text = "Test" });
			var handler = new ButtonHandlerStub();
			handler.SetMauiContext(MauiContext);
			button.Handler = handler;

			await InvokeOnMainThreadAsync(() =>
			{
				var dispatcher = button.FindDispatcher();
				dispatcher.DispatchIfRequired(() =>
				{
					executed = true;
				});
			});

			Assert.True(executed);
		}

		[Fact]
		public async Task DispatchIfRequiredAsync_OnMainThread_ExecutesSynchronously()
		{
			bool executed = false;
			var button = await InvokeOnMainThreadAsync(() => new Button { Text = "Test" });
			var handler = new ButtonHandlerStub();
			handler.SetMauiContext(MauiContext);
			button.Handler = handler;

			await InvokeOnMainThreadAsync(async () =>
			{
				var dispatcher = button.FindDispatcher();
				await dispatcher.DispatchIfRequiredAsync(() =>
				{
					executed = true;
				});
			});

			Assert.True(executed);
		}

		[Fact]
		public async Task DispatchIfRequired_FromBackgroundThread_DispatchesToMainThread()
		{
			bool executed = false;
			var button = await InvokeOnMainThreadAsync(() => new Button { Text = "Test" });
			var handler = new ButtonHandlerStub();
			handler.SetMauiContext(MauiContext);
			button.Handler = handler;

			await Task.Run(async () =>
			{
				var dispatcher = button.FindDispatcher();
				dispatcher.DispatchIfRequired(() =>
				{
					executed = true;
				});

				// Wait a bit for the dispatch to complete
				await Task.Delay(500);
			});

			Assert.True(executed);
		}

		public class ApplicationHandlerStub : ElementHandler<Application, object>
		{
			public ApplicationHandlerStub() : base(ElementHandler.ElementMapper)
			{

			}

			protected override object CreatePlatformElement() => new Object();
		}

		public class ButtonHandlerStub : ElementHandler<Button, object>
		{
			public ButtonHandlerStub() : base(ElementHandler.ElementMapper)
			{

			}

			protected override object CreatePlatformElement() => new Object();
		}
	}
}
