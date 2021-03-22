using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Tests;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	[Category(TestCategory.Core, TestCategory.Hosting)]
	public class HostBuilderHandlerTests
	{
		[Fact]
		public void CanBuildAHost()
		{
			var host = AppHostBuilder
				.CreateDefaultAppBuilder()
				.Build();

			Assert.NotNull(host);
		}

		[Fact]
		public void CanGetIMauiHandlersServiceProviderFromServices()
		{
			var host = AppHostBuilder
				.CreateDefaultAppBuilder()
				.Build();

			Assert.NotNull(host);
			Assert.NotNull(host.Services);
			Assert.NotNull(host.Handlers);
			Assert.IsType<MauiHandlersServiceProvider>(host.Handlers);
			Assert.Equal(host.Handlers, host.Services.GetService<IMauiHandlersServiceProvider>());
		}

		[Fact]
		public void CanRegisterAndGetHandler()
		{
			var host = AppHostBuilder
				.CreateDefaultAppBuilder()
				.RegisterHandler<IViewStub, ViewHandlerStub>()
				.Build();

			var handler = host.Handlers.GetHandler(typeof(IViewStub));

			Assert.NotNull(handler);
			Assert.IsType<ViewHandlerStub>(handler);
		}

		[Fact]
		public void CanRegisterAndGetHandlerWithDictionary()
		{
			var host = AppHostBuilder
				.CreateDefaultAppBuilder()
				.RegisterHandlers(new Dictionary<Type, Type>
				{
					{ typeof(IViewStub), typeof(ViewHandlerStub) }
				})
				.Build();

			var handler = host.Handlers.GetHandler(typeof(IViewStub));

			Assert.NotNull(handler);
			Assert.IsType<ViewHandlerStub>(handler);
		}

		[Fact]
		public void CanRegisterAndGetHandlerForType()
		{
			var host = AppHostBuilder
				.CreateDefaultAppBuilder()
				.RegisterHandler<IViewStub, ViewHandlerStub>()
				.Build();

			var handler = host.Handlers.GetHandler(typeof(ViewStub));

			Assert.NotNull(handler);
			Assert.IsType<ViewHandlerStub>(handler);
		}

		[Fact]
		public void DefaultHandlersAreRegistered()
		{
			var host = AppHostBuilder
				.CreateDefaultAppBuilder()
				.Build();

			var handler = host.Handlers.GetHandler(typeof(IButton));

			Assert.NotNull(handler);
			Assert.IsType<ButtonHandler>(handler);
		}

		[Fact]
		public void CanSpecifyHandler()
		{
			var host = AppHostBuilder
				.CreateDefaultAppBuilder()
				.RegisterHandler<ButtonStub, ButtonHandlerStub>()
				.Build();

			var defaultHandler = host.Handlers.GetHandler(typeof(IButton));
			var specificHandler = host.Handlers.GetHandler(typeof(ButtonStub));

			Assert.NotNull(defaultHandler);
			Assert.NotNull(specificHandler);
			Assert.IsType<ButtonHandler>(defaultHandler);
			Assert.IsType<ButtonHandlerStub>(specificHandler);
		}
	}
}