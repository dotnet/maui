using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.UnitTests.Hosting
{
	[Category(TestCategory.Core, TestCategory.Hosting)]
	public class HostBuilderHandlerTests
	{
		[Fact]
		public void CanBuildAHost()
		{
			var host = AppHost
				.CreateDefaultBuilder()
				.Build();

			Assert.NotNull(host);
		}

		[Fact]
		public void CanGetIMauiHandlersServiceProviderFromServices()
		{
			var host = AppHost
				.CreateDefaultBuilder()
				.Build();

			Assert.NotNull(host);
			Assert.NotNull(host.Services);
			Assert.NotNull(host.Handlers);
			Assert.IsType<Maui.Hosting.Internal.MauiHandlersServiceProvider>(host.Handlers);
			Assert.Equal(host.Handlers, host.Services.GetService<IMauiHandlersServiceProvider>());
		}

		[Fact]
		public void CanRegisterAndGetHandlerUsingType()
		{
			var host = AppHost
				.CreateDefaultBuilder()
				.ConfigureMauiHandlers((_, handlers) => handlers.AddHandler<IViewStub, ViewHandlerStub>())
				.Build();

			var handler = host.Handlers.GetHandler(typeof(IViewStub));

			Assert.NotNull(handler);
			Assert.IsType<ViewHandlerStub>(handler);
		}

		[Fact]
		public void CanRegisterAndGetHandler()
		{
			var host = AppHost
				.CreateDefaultBuilder()
				.ConfigureMauiHandlers((_, handlers) => handlers.AddHandler<IViewStub, ViewHandlerStub>())
				.Build();

			var handler = host.Handlers.GetHandler<IViewStub>();

			Assert.NotNull(handler);
			Assert.IsType<ViewHandlerStub>(handler);
		}

		[Fact]
		public void CanRegisterAndGetHandlerWithType()
		{
			var host = AppHost
				.CreateDefaultBuilder()
				.ConfigureMauiHandlers((_, handlers) => handlers.AddHandler(typeof(IViewStub), typeof(ViewHandlerStub)))
				.Build();

			var handler = host.Handlers.GetHandler(typeof(IViewStub));

			Assert.NotNull(handler);
			Assert.IsType<ViewHandlerStub>(handler);
		}

		[Fact]
		public void CanRegisterAndGetHandlerWithDictionary()
		{
			var dic = new Dictionary<Type, Type>
			{
				{ typeof(IViewStub), typeof(ViewHandlerStub) }
			};

			var host = AppHost
				.CreateDefaultBuilder()
				.ConfigureMauiHandlers((_, handlers) => handlers.AddHandlers(dic))
				.Build();

			var handler = host.Handlers.GetHandler(typeof(IViewStub));

			Assert.NotNull(handler);
			Assert.IsType<ViewHandlerStub>(handler);
		}

		[Fact]
		public void CanRegisterAndGetHandlerForConcreteType()
		{
			var host = AppHost
				.CreateDefaultBuilder()
				.ConfigureMauiHandlers((_, handlers) => handlers.AddHandler<IViewStub, ViewHandlerStub>())
				.Build();

			var handler = host.Handlers.GetHandler(typeof(ViewStub));

			Assert.NotNull(handler);
			Assert.IsType<ViewHandlerStub>(handler);
		}

		[Fact]
		public void DefaultHandlersAreRegistered()
		{
			var host = AppHost
				.CreateDefaultBuilder()
				.Build();

			var handler = host.Handlers.GetHandler(typeof(IButton));

			Assert.NotNull(handler);
			Assert.IsType<ButtonHandler>(handler);
		}

		[Fact]
		public void CanSpecifyHandler()
		{
			var host = AppHost
				.CreateDefaultBuilder()
				.ConfigureMauiHandlers((_, handlers) => handlers.AddHandler<ButtonStub, ButtonHandlerStub>())
				.Build();

			var defaultHandler = host.Handlers.GetHandler(typeof(IButton));
			var specificHandler = host.Handlers.GetHandler(typeof(ButtonStub));

			Assert.NotNull(defaultHandler);
			Assert.NotNull(specificHandler);
			Assert.IsType<ButtonHandler>(defaultHandler);
			Assert.IsType<ButtonHandlerStub>(specificHandler);
		}

		[Fact]
		public void CanChangeHandlerRegistration()
		{
			var host = AppHost
				.CreateDefaultBuilder()
				.ConfigureMauiHandlers((_, handlers) => handlers.AddHandler<ButtonStub, ButtonHandlerStub>())
				.Build();

			var specificHandler = host.Handlers.GetHandler(typeof(ButtonStub));
			Assert.IsType<ButtonHandlerStub>(specificHandler);

			host.Handlers.GetCollection().AddHandler<ButtonStub, AlternateButtonHandlerStub>();

			var alternateHandler = host.Handlers.GetHandler(typeof(ButtonStub));
			Assert.IsType<AlternateButtonHandlerStub>(alternateHandler);
		}
	}
}