using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.UnitTests.Hosting
{
	[Category(TestCategory.Core, TestCategory.Hosting)]
	public class HostBuilderHandlerTests
	{
		[Fact]
		public void HostBuilderCanBuildAHost()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.Build();

			Assert.NotNull(mauiApp.Services);
		}

		[Fact]
		public void HostBuilderWithDefaultsRegistersMauiHandlersFactory()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.Build();

			Assert.NotNull(mauiApp.Services);
			var handlers = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();
			Assert.NotNull(handlers);
			Assert.IsType<Maui.Hosting.Internal.MauiHandlersFactory>(handlers);
		}

		[Fact]
		public void HostBuilderWithoutDefaultsDoesNotRegisterMauiHandlersFactory()
		{
			var mauiApp = MauiApp.CreateBuilder(false)
				.Build();

			Assert.NotNull(mauiApp.Services);
			Assert.Throws<InvalidOperationException>(() => mauiApp.Services.GetRequiredService<IMauiHandlersFactory>());
		}

		[Theory]
		[InlineData(true, true)]
		[InlineData(true, false)]
		[InlineData(false, true)]
		[InlineData(false, false)]
		public void HostBuilderCanRegisterAndResolveCorrespondingHandlerService(bool registerHandlerServicesWithGenerics, bool retrieveHandlerServiceWithGenerics)
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers =>
				{
					if (registerHandlerServicesWithGenerics)
						handlers.AddHandler<IViewStub, ViewHandlerStub>();
					else
						handlers.AddHandler(typeof(IViewStub), typeof(ViewHandlerStub));
				})
				.Build();

			var mauiHandlersFactory = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			var handlerService = retrieveHandlerServiceWithGenerics ? mauiHandlersFactory.GetHandler<IViewStub>() : mauiHandlersFactory.GetHandler(typeof(IViewStub));

			Assert.NotNull(handlerService);
			Assert.IsType<ViewHandlerStub>(handlerService);
		}

		[Fact]
		public void HostBuilderResolvesLastRegisteredHandlerServiceForServiceType()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<ButtonStub, ButtonHandlerStub>())
				.Build();

			var mauiHandlersFactory = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();
			var specificHandler = mauiHandlersFactory.GetHandler(typeof(ButtonStub));
			Assert.IsType<ButtonHandlerStub>(specificHandler);

			var collection = mauiHandlersFactory.GetCollection();

			collection.AddHandler<ButtonStub, AlternateButtonHandlerStub>();

			var alternateHandler = mauiHandlersFactory.GetHandler(typeof(ButtonStub));
			Assert.IsType<AlternateButtonHandlerStub>(alternateHandler);
		}

		[Fact]
		public void HostBuilderThrowsWhenNoMatchingHandlerServiceTypeIsRegistered()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.Build();

			var mauiHandlersFactory = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			Assert.Throws<HandlerNotFoundException>(() => mauiHandlersFactory.GetHandler(typeof(ViewStub)));
		}
	}
}