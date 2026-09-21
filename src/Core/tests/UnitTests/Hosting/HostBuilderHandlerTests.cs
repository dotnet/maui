using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.HotReload;
using Microsoft.Maui.Platform;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.UnitTests.Hosting
{
	[Category(TestCategory.Core, TestCategory.Hosting)]
	[Collection(MainThreadStaticStateCollection.Name)]
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
		public void HandlerCollectionFactoriesReceiveTheCurrentMauiContext()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddTransient(services => new ContextAwareService(services.GetRequiredService<IMauiContext>()));
					handlers.AddHandler<IViewStub>(services => new ContextAwareHandlerStub(services.GetRequiredService<ContextAwareService>()));
				})
				.Build();
			var mauiContext = new MauiContext(mauiApp.Services);

			var handler = Assert.IsType<ContextAwareHandlerStub>(new ViewStub().ToHandler(mauiContext));

			Assert.Same(mauiContext, handler.ContextAwareService.MauiContext);
		}

		[Fact]
		public void HandlerCollectionFactoriesReceiveTheCurrentActivationServiceProvider()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<IViewStub>(services =>
					{
						var activationServices = services.GetRequiredService<IServiceProvider>();
						return new ContextAwareHandlerStub(new ContextAwareService(activationServices.GetRequiredService<IMauiContext>()));
					});
				})
				.Build();
			var mauiContext = new MauiContext(mauiApp.Services);

			var handler = Assert.IsType<ContextAwareHandlerStub>(new ViewStub().ToHandler(mauiContext));

			Assert.Same(mauiContext, handler.ContextAwareService.MauiContext);
		}

		[Fact]
		public void ApplicationHandlerFactoriesReceiveTheCurrentMauiContext()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers =>
					handlers.AddHandler<IApplication>(services =>
						new ContextAwareElementHandlerStub<IApplication>(
							new ContextAwareService(services.GetRequiredService<IMauiContext>()))))
				.Build();
			var mauiContext = new MauiContext(mauiApp.Services);
			var application = Substitute.For<IApplication>();

			new object().SetApplicationHandler(application, mauiContext);

			var handler = Assert.IsType<ContextAwareElementHandlerStub<IApplication>>(application.Handler);
			Assert.Same(mauiContext, handler.ContextAwareService.MauiContext);
		}

		[Fact]
		public void WindowHandlerFactoriesReceiveTheCurrentMauiContext()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers =>
					handlers.AddHandler<IWindow>(services =>
						new ContextAwareElementHandlerStub<IWindow>(
							new ContextAwareService(services.GetRequiredService<IMauiContext>()))))
				.Build();
			var mauiContext = new MauiContext(mauiApp.Services);
			var window = Substitute.For<IWindow>();

			new object().SetWindowHandler(window, mauiContext);

			var handler = Assert.IsType<ContextAwareElementHandlerStub<IWindow>>(window.Handler);
			Assert.Same(mauiContext, handler.ContextAwareService.MauiContext);
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

		[Theory]
		[InlineData(typeof(IViewStub))]
		[InlineData(typeof(IMyBaseViewStub))]
		[InlineData(typeof(IMyDerivedViewStub))]
		public void HostBuilderResolvesHandlerRegisteredUnderBaseInterfaceType(Type baseInterfaceType)
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler(baseInterfaceType, typeof(ViewHandlerStub)))
				.Build();

			var mauiHandlersFactory = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			var handlerService = mauiHandlersFactory.GetHandler(typeof(MyDerivedViewStub));

			Assert.NotNull(handlerService);
			Assert.IsType<ViewHandlerStub>(handlerService);
		}

		[Fact]
		public void HostBuilderResolvesToHandlerRegisteredUnderMostDerivedBaseInterfaceType()
		{
			var mostDerivedInterfaceHandler = new ViewHandlerStub();
			var lessDerivedInterfaceHandler = new ViewHandlerStub();
			var primaryInterfaceHandler = new ViewHandlerStub();

			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<IMyDerivedViewStub>(_ => mostDerivedInterfaceHandler);
					handlers.AddHandler<IMyBaseViewStub>(_ => lessDerivedInterfaceHandler);
					handlers.AddHandler<IViewStub>(_ => primaryInterfaceHandler);
				})
				.Build();

			var mauiHandlersFactory = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			var handlerService = mauiHandlersFactory.GetHandler(typeof(MyDerivedViewStub));

			Assert.NotNull(handlerService);
			Assert.Same(mostDerivedInterfaceHandler, handlerService);
		}

		[Fact]
		public void HostBuilderResolvesToHandlerRegisteredUnderConcreteTypeOverInterfaceType()
		{
			var interfaceHandler = new ViewHandlerStub();
			var classHandler = new ViewHandlerStub();

			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<ViewStub>(_ => classHandler);
					handlers.AddHandler<IMyDerivedViewStub>(_ => interfaceHandler);
					handlers.AddHandler<IMyBaseViewStub>(_ => interfaceHandler);
					handlers.AddHandler<IViewStub>(_ => interfaceHandler);
				})
				.Build();

			var mauiHandlersFactory = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			var handlerService = mauiHandlersFactory.GetHandler(typeof(MyDerivedViewStub));

			Assert.NotNull(handlerService);
			Assert.IsType<ViewHandlerStub>(handlerService);
			Assert.Same(classHandler, handlerService);
		}

		[Fact]
		public void HostBuilderDoesNotResolveHandlersRegisteredUnderMoreDerivedTypes()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<MyBaseViewStub, ViewHandlerStub>();
					handlers.AddHandler<MyDerivedViewStub, ViewHandlerStub>();
				})
				.Build();

			var mauiHandlersFactory = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			Assert.Throws<HandlerNotFoundException>(() => mauiHandlersFactory.GetHandler(typeof(ViewStub)));
		}

		[Theory]
		[InlineData(typeof(IViewStub), typeof(ViewHandlerStub))]
		[InlineData(typeof(ViewStub), typeof(ViewHandlerStub))]
		[InlineData(typeof(MyBaseViewStub), typeof(MyBaseHandlerStub))]
		[InlineData(typeof(MyDerivedViewStub), typeof(MyBaseHandlerStub))]
		public void HostBuilderResolvesClosestApplicableServiceType(Type type, Type expectedHandlerType)
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<IViewStub, ViewHandlerStub>();
					handlers.AddHandler<MyBaseViewStub, MyBaseHandlerStub>();
				})
				.Build();

			var mauiHandlersFactory = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			var handlerService = mauiHandlersFactory.GetHandler(type);

			Assert.NotNull(handlerService);
			Assert.IsType(expectedHandlerType, handlerService);
		}

		interface IMyBaseViewStub : IViewStub { }
		interface IMyDerivedViewStub : IMyBaseViewStub { }
		class MyBaseViewStub : ViewStub, IMyBaseViewStub { }
		class MyDerivedViewStub : MyBaseViewStub, IMyDerivedViewStub { }
		class MyBaseHandlerStub : ViewHandlerStub { }

		[Fact]
		public void HostBuilderThrowsWhenOnlyInterfacesRelatedByInheritanceAreRegistered()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<IParentAViewStub, ViewHandlerStub>();
					handlers.AddHandler<IParentBViewStub, ViewHandlerStub>();
				})
				.Build();

			var mauiHandlersFactory = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			Assert.Throws<InvalidOperationException>(() => mauiHandlersFactory.GetHandler(typeof(ChildViewStub)));
		}

		[Fact]
		public void HostBuilderResolvesToHandlerRegisteredUnderConcreteTypeDespiteAmbiguousInterfacesRegistered()
		{
			var interfaceHandler = new ViewHandlerStub();
			var alternateInterfaceHandler = new ViewHandlerStub();
			var classHandler = new ViewHandlerStub();

			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<ViewStub>(_ => classHandler);
					handlers.AddHandler<IParentAViewStub>(_ => alternateInterfaceHandler);
					handlers.AddHandler<IParentBViewStub>(_ => interfaceHandler);
				})
				.Build();

			var mauiHandlersFactory = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			var handlerService = mauiHandlersFactory.GetHandler(typeof(ChildViewStub));

			Assert.NotNull(handlerService);
			Assert.Same(classHandler, handlerService);
		}

		class ChildViewStub : ViewStub, IParentAViewStub, IParentBViewStub { }
		interface IParentAViewStub : IViewStub { }
		interface IParentBViewStub : IViewStub { }

		[Fact]
		public void HostBuilderResolvesHandlerTypeForServiceRegisteredWithType()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<IViewStub, ViewHandlerStub>())
				.Build();

			var mauiHandlersFactory = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			Type handlerType = mauiHandlersFactory.GetHandlerType(typeof(ViewStub));
			Assert.Same(typeof(ViewHandlerStub), handlerType);
		}

		[Fact]
		public void HostBuilderCannotResolveHandlerTypeForServiceRegisteredWithFactory()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<IViewStub>(_ => new ViewHandlerStub()))
				.Build();

			var mauiHandlersFactory = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			Type handlerType = mauiHandlersFactory.GetHandlerType(typeof(ViewStub));
			Assert.Null(handlerType);
		}

		[Fact]
		public void SecondConfigureMauiHandlersCallReplacesHandler()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<IViewStub, ViewHandlerStub>())
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<IViewStub, AlternateViewHandlerStub>())
				.Build();

			var mauiHandlersFactory = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			var handlerService = mauiHandlersFactory.GetHandler(typeof(ViewStub));
			Assert.NotNull(handlerService);
			Assert.IsType<AlternateViewHandlerStub>(handlerService);
		}

		[Fact]
		public void FactoryBasedHandlerRegistrationReplacesHandler()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<IViewStub, ViewHandlerStub>())
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<IViewStub>(_ => new AlternateViewHandlerStub()))
				.Build();

			var mauiHandlersFactory = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			var handlerService = mauiHandlersFactory.GetHandler(typeof(ViewStub));
			Assert.NotNull(handlerService);
			Assert.IsType<AlternateViewHandlerStub>(handlerService);
		}

		[Fact]
		public void FactoryBasedHandlerRegistrationAppliesPendingHotReloadReplacement()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<IViewStub>(_ => new ViewHandlerStub()))
				.Build();
			var mauiHandlersFactory = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();
			MauiHotReloadHelper.RegisterHandlers(mauiHandlersFactory.GetCollection());

			try
			{
				MauiHotReloadHelper.RegisterHandlerReplacement(typeof(ViewHandlerStub).FullName!, typeof(AlternateViewHandlerStub));

				var initialHandler = mauiHandlersFactory.GetHandler(typeof(ViewStub));
				var replacedHandler = mauiHandlersFactory.GetHandler(typeof(ViewStub));

				Assert.IsType<ViewHandlerStub>(initialHandler);
				Assert.IsType<AlternateViewHandlerStub>(replacedHandler);
			}
			finally
			{
				MauiHotReloadHelper.Reset();
			}
		}

		[Fact]
		public void FactoryBasedHandlerRegistrationAppliesPendingHotReloadReplacementOnce()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<IViewStub>(_ => new ViewHandlerStub()))
				.Build();
			var mauiHandlersFactory = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();
			var handlers = mauiHandlersFactory.GetCollection();
			MauiHotReloadHelper.RegisterHandlers(handlers);

			try
			{
				MauiHotReloadHelper.RegisterHandlerReplacement(typeof(ViewHandlerStub).FullName!, typeof(ViewHandlerStub));
				var initialRegistrationCount = handlers.Count;

				for (var i = 0; i < 5; i++)
					Assert.IsType<ViewHandlerStub>(mauiHandlersFactory.GetHandler(typeof(ViewStub)));

				Assert.Equal(initialRegistrationCount + 1, handlers.Count);
			}
			finally
			{
				MauiHotReloadHelper.Reset();
			}
		}

		class AlternateViewHandlerStub : ViewHandlerStub
		{
		}

		class ContextAwareHandlerStub : ViewHandlerStub
		{
			public ContextAwareHandlerStub(ContextAwareService contextAwareService)
			{
				ContextAwareService = contextAwareService;
			}

			public ContextAwareService ContextAwareService { get; }
		}

		class ContextAwareElementHandlerStub<TVirtualView> : ElementHandler<TVirtualView, object>
			where TVirtualView : class, IElement
		{
			public ContextAwareElementHandlerStub(ContextAwareService contextAwareService)
				: base(new PropertyMapper<TVirtualView>())
			{
				ContextAwareService = contextAwareService;
			}

			public ContextAwareService ContextAwareService { get; }

			protected override object CreatePlatformElement() => new object();
		}

		class ContextAwareService
		{
			public ContextAwareService(IMauiContext mauiContext)
			{
				MauiContext = mauiContext;
			}

			public IMauiContext MauiContext { get; }
		}
	}
}