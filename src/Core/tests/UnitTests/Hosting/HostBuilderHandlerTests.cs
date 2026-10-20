using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
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

			Assert.IsType<ViewHandlerStub>(mauiHandlersFactory.GetHandler(typeof(ViewStub)));
			Assert.Null(mauiHandlersFactory.GetHandlerType(typeof(ViewStub)));
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

		[Fact]
		public async Task ActivatedHandlerTypeMetadataDoesNotRootServiceDescriptor()
		{
			try
			{
				var descriptorReference = RegisterActivatedHandlerType();

				Assert.False(await descriptorReference.WaitForCollect(), "Activated handler metadata should not keep service descriptors alive.");
			}
			finally
			{
				MauiHotReloadHelper.Reset();
			}

			static WeakReference RegisterActivatedHandlerType()
			{
				var descriptor = ServiceDescriptor.Transient(typeof(IViewStub), static _ => new ViewHandlerStub());
				MauiHotReloadHelper.RegisterHandlerType(descriptor, typeof(ViewHandlerStub));
				return new WeakReference(descriptor);
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

		[Fact]
		public void HostBuilderResolvesHandlerDeclaredWithElementHandlerAttribute()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.Build();

			var mauiHandlersFactory = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();
			var handler = mauiHandlersFactory.GetHandler(typeof(AttributedViewStub));

			Assert.IsType<AttributedViewHandlerStub>(handler);
		}

		[Fact]
		public void HostBuilderResolvesBaseElementHandlerAttribute()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.Build();

			var mauiHandlersFactory = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();
			var handler = mauiHandlersFactory.GetHandler(typeof(DerivedAttributedViewStub));

			Assert.IsType<AttributedViewHandlerStub>(handler);
		}

		[Fact]
		public void HostBuilderPrefersRegisteredHandlerOverElementHandlerAttribute()
		{
			var registeredHandler = new ViewHandlerStub();
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<AttributedViewStub>(_ => registeredHandler))
				.Build();

			var mauiHandlersFactory = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			var handler = mauiHandlersFactory.GetHandler(typeof(AttributedViewStub));

			Assert.Same(registeredHandler, handler);
		}

		[Fact]
		public void HostBuilderCannotResolveHandlerTypeForAttributedServiceRegisteredWithFactory()
		{
			var registeredHandler = new AlternateAttributedViewHandlerStub();
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<AttributedViewStub>(_ => registeredHandler))
				.Build();

			var mauiHandlersFactory = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			var handler = mauiHandlersFactory.GetHandler(typeof(AttributedViewStub));
			var handlerType = mauiHandlersFactory.GetHandlerType(typeof(AttributedViewStub));

			Assert.Same(registeredHandler, handler);
			Assert.Null(handlerType);
		}

		[Fact]
		public void HostBuilderPrefersDirectElementHandlerAttributeOverAssignableFactoryRegistration()
		{
			var registeredHandler = new AlternateAttributedViewHandlerStub();
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<IViewStub>(_ => registeredHandler))
				.Build();

			var mauiHandlersFactory = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			var handler = mauiHandlersFactory.GetHandler(typeof(AttributedViewStub));
			var handlerType = mauiHandlersFactory.GetHandlerType(typeof(AttributedViewStub));

			Assert.IsType<AttributedViewHandlerStub>(handler);
			Assert.Same(typeof(AttributedViewHandlerStub), handlerType);
		}

		[Fact]
		public void HostBuilderPrefersRegisteredBaseHandlerOverBaseElementHandlerAttribute()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<AttributedViewStub, AlternateAttributedViewHandlerStub>())
				.Build();

			var mauiHandlersFactory = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			var handler = mauiHandlersFactory.GetHandler(typeof(DerivedAttributedViewStub));
			var handlerType = mauiHandlersFactory.GetHandlerType(typeof(DerivedAttributedViewStub));

			Assert.IsType<AlternateAttributedViewHandlerStub>(handler);
			Assert.Same(typeof(AlternateAttributedViewHandlerStub), handlerType);
		}

		[Fact]
		public void HostBuilderCannotResolveHandlerTypeForInheritedAttributedServiceRegisteredWithFactory()
		{
			var registeredHandler = new AlternateAttributedViewHandlerStub();
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<AttributedViewStub>(_ => registeredHandler))
				.Build();

			var mauiHandlersFactory = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			var handler = mauiHandlersFactory.GetHandler(typeof(DerivedAttributedViewStub));
			var handlerType = mauiHandlersFactory.GetHandlerType(typeof(DerivedAttributedViewStub));

			Assert.Same(registeredHandler, handler);
			Assert.Null(handlerType);
		}

		[Fact]
		public void HostBuilderPrefersDirectElementHandlerAttributeOverRegisteredBaseHandler()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<ViewStub, ViewHandlerStub>())
				.Build();

			var mauiHandlersFactory = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			var handler = mauiHandlersFactory.GetHandler(typeof(OverrideAttributedViewStub));
			var handlerType = mauiHandlersFactory.GetHandlerType(typeof(OverrideAttributedViewStub));

			Assert.IsType<AlternateAttributedViewHandlerStub>(handler);
			Assert.Same(typeof(AlternateAttributedViewHandlerStub), handlerType);
		}

		[Fact]
		public void HostBuilderPrefersDirectElementHandlerAttributeOverRegisteredInterfaceHandler()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<IViewStub, AlternateAttributedViewHandlerStub>())
				.Build();

			var mauiHandlersFactory = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			var handler = mauiHandlersFactory.GetHandler(typeof(AttributedViewStub));
			var handlerType = mauiHandlersFactory.GetHandlerType(typeof(AttributedViewStub));

			Assert.IsType<AttributedViewHandlerStub>(handler);
			Assert.Same(typeof(AttributedViewHandlerStub), handlerType);
		}

		[Fact]
		public void HostBuilderThrowsActionableExceptionForElementHandlerAttributeWithoutDefaultConstructor()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.Build();

			var mauiHandlersFactory = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			var exception = Assert.Throws<HandlerNotFoundException>(() => mauiHandlersFactory.GetHandler(typeof(AttributedViewHandlerWithoutDefaultConstructorViewStub)));

			Assert.IsType<MissingMethodException>(exception.InnerException);
			Assert.Contains("public parameterless constructor", exception.Message, StringComparison.Ordinal);
			Assert.Contains(nameof(IMauiHandlersFactory.GetHandler), exception.Message, StringComparison.Ordinal);
			Assert.Contains("ToHandler()", exception.Message, StringComparison.Ordinal);
		}

		[Fact]
		public void ToHandlerUsesActivatorUtilitiesForElementHandlerAttributeWithoutDefaultConstructor()
		{
			var dependency = new ConstructorDependency();
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton(dependency);
			var mauiApp = builder.Build();
			var mauiContext = new MauiContext(mauiApp.Services);

			var handler = Assert.IsType<AttributedViewHandlerWithoutDefaultConstructorStub>(
				new AttributedViewHandlerWithoutDefaultConstructorViewStub().ToHandler(mauiContext));

			Assert.Same(dependency, handler.Dependency);
		}

		[Fact]
		public void ConstructorInjectionUsesCurrentContextAndHonorsFactoryOverrides()
		{
			using var app = MauiApp.CreateBuilder().Build();
			var firstContext = new MauiContext(app.Services);
			var secondContext = new MauiContext(app.Services);
			var firstDependency = new ConstructorDependency();
			var secondDependency = new ConstructorDependency();
			firstContext.AddSpecific(firstDependency);
			secondContext.AddSpecific(secondDependency);

			Assert.Same(firstDependency, Assert.IsType<AttributedViewHandlerWithoutDefaultConstructorStub>(
				new AttributedViewHandlerWithoutDefaultConstructorViewStub().ToHandler(firstContext)).Dependency);
			Assert.Same(secondDependency, Assert.IsType<AttributedViewHandlerWithoutDefaultConstructorStub>(
				new AttributedViewHandlerWithoutDefaultConstructorViewStub().ToHandler(secondContext)).Dependency);

			var calls = 0;
			firstContext.Handlers.GetCollection().AddHandler<AttributedViewHandlerWithoutDefaultConstructorViewStub>(_ =>
			{
				calls++;
				return new AlternateAttributedViewHandlerStub();
			});
			Assert.IsType<AlternateAttributedViewHandlerStub>(
				new AttributedViewHandlerWithoutDefaultConstructorViewStub().ToHandler(firstContext));
			Assert.IsType<AlternateAttributedViewHandlerStub>(
				new AttributedViewHandlerWithoutDefaultConstructorViewStub().ToHandler(secondContext));
			Assert.Equal(2, calls);
		}

		[Fact]
		public void ConstructorInjectionFailureIsNotRetried()
		{
			var calls = 0;
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddTransient<ConstructorDependency>(_ =>
			{
				calls++;
				throw new MissingMethodException("Dependency factory failed.");
			});
			using var app = builder.Build();
			var context = new MauiContext(app.Services);

			Assert.Throws<MissingMethodException>(() =>
				new AttributedViewHandlerWithoutDefaultConstructorViewStub().ToHandler(context));
			Assert.Equal(1, calls);

			context.AddSpecific(new ConstructorDependency());
			Assert.IsType<AttributedViewHandlerWithoutDefaultConstructorStub>(
				new AttributedViewHandlerWithoutDefaultConstructorViewStub().ToHandler(context));
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void ToHandlerPrefersParameterlessConstructorWhenInjectionIsAlsoAvailable(bool registered)
		{
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddTransient<ConstructorDependency>(_ =>
				throw new InvalidOperationException("The parameterless constructor must not resolve dependencies."));
			if (registered)
				builder.ConfigureMauiHandlers(handlers => handlers.AddHandler<ViewStub, HandlerWithBothConstructors>());
			using var app = builder.Build();
			var context = new MauiContext(app.Services);
			var view = registered ? new ViewStub() : new AttributedViewWithBothConstructorsStub();

			var handler = Assert.IsType<HandlerWithBothConstructors>(view.ToHandler(context));

			Assert.False(handler.UsedInjection);
		}

		[Fact]
		public void ToHandlerCreatesValueTypeHandlerWithoutReflectedParameterlessConstructor()
		{
			using var app = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<IElement, ValueTypeHandler>())
				.Build();
			var context = new MauiContext(app.Services);
			var view = Substitute.For<IElement>();

			var handler = Assert.IsType<ValueTypeHandler>(view.ToHandler(context));

			Assert.Same(context, handler.MauiContext);
			Assert.Same(view, handler.VirtualView);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void ConstructorInjectionAvoidsMissingConstructorExceptionsIncludingAfterInvalidation(bool registered)
		{
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton(new ConstructorDependency());
			if (registered)
				builder.ConfigureMauiHandlers(handlers => handlers.AddHandler<ViewStub, AttributedViewHandlerWithoutDefaultConstructorStub>());
			using var app = builder.Build();
			var context = new MauiContext(app.Services);
			var exceptions = 0;
			var threadId = Environment.CurrentManagedThreadId;
			EventHandler<FirstChanceExceptionEventArgs> onException = (_, args) =>
			{
				if (Environment.CurrentManagedThreadId == threadId && args.Exception is MissingMethodException)
					exceptions++;
			};
			IElementHandler Activate() => registered
				? new ViewStub().ToHandler(context)
				: new AttributedViewHandlerWithoutDefaultConstructorViewStub().ToHandler(context);

			AppDomain.CurrentDomain.FirstChanceException += onException;
			try
			{
				Assert.IsType<AttributedViewHandlerWithoutDefaultConstructorStub>(Activate());
				Assert.Equal(0, exceptions);
				Assert.IsType<AttributedViewHandlerWithoutDefaultConstructorStub>(Activate());
				Assert.Equal(0, exceptions);

				context.Handlers.GetCollection().AddHandler<ButtonStub>(_ => new ButtonHandlerStub());
				Assert.IsType<AttributedViewHandlerWithoutDefaultConstructorStub>(Activate());
				Assert.Equal(0, exceptions);

				MauiHotReloadHelper.ClearCache(Array.Empty<Type>());
				Assert.IsType<AttributedViewHandlerWithoutDefaultConstructorStub>(Activate());
				Assert.Equal(0, exceptions);
			}
			finally
			{
				AppDomain.CurrentDomain.FirstChanceException -= onException;
				MauiHotReloadHelper.Reset();
			}

			if (registered)
				Assert.Throws<MissingMethodException>(() => context.Handlers.GetHandler(typeof(ViewStub)));
			else
				Assert.Throws<HandlerNotFoundException>(() => context.Handlers.GetHandler(typeof(AttributedViewHandlerWithoutDefaultConstructorViewStub)));
		}

		[Fact]
		public void ConstructorInjectionDoesNotAffectOtherApps()
		{
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton(new ConstructorDependency());
			using var firstApp = builder.Build();
			new AttributedViewHandlerWithoutDefaultConstructorViewStub().ToHandler(new MauiContext(firstApp.Services));

			using var secondApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<AttributedViewHandlerWithoutDefaultConstructorViewStub>(
					_ => new AlternateAttributedViewHandlerStub()))
				.Build();
			Assert.IsType<AlternateAttributedViewHandlerStub>(
				new AttributedViewHandlerWithoutDefaultConstructorViewStub().ToHandler(new MauiContext(secondApp.Services)));
		}

		[Fact]
		public void HotReloadReplacesHandlerAfterConstructorInjection()
		{
			var builder = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<ViewStub, AttributedViewHandlerWithoutDefaultConstructorStub>());
			builder.Services.AddSingleton(new ConstructorDependency());
			using var app = builder.Build();
			var context = new MauiContext(app.Services);
			MauiHotReloadHelper.RegisterHandlers(context.Handlers.GetCollection());
			try
			{
				Assert.IsType<AttributedViewHandlerWithoutDefaultConstructorStub>(new ViewStub().ToHandler(context));
				Assert.IsType<AttributedViewHandlerWithoutDefaultConstructorStub>(new ViewStub().ToHandler(context));

				MauiHotReloadHelper.RegisterHandlerReplacement(
					typeof(AttributedViewHandlerWithoutDefaultConstructorStub).FullName!,
					typeof(AlternateAttributedViewHandlerStub));

				Assert.IsType<AlternateAttributedViewHandlerStub>(new ViewStub().ToHandler(context));
			}
			finally
			{
				MauiHotReloadHelper.Reset();
			}
		}

		[Fact]
		public void RemovingLatestDescriptorRestoresPreviousRegistration()
		{
			using var app = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<AttributedViewStub, AttributedViewHandlerStub>())
				.Build();
			var factory = app.Services.GetRequiredService<IMauiHandlersFactory>();
			var collection = factory.GetCollection();
			var descriptor = ServiceDescriptor.Transient(typeof(AttributedViewStub), typeof(AlternateAttributedViewHandlerStub));
			collection.Add(descriptor);
			Assert.IsType<AlternateAttributedViewHandlerStub>(factory.GetHandler(typeof(DerivedAttributedViewStub)));
			Assert.True(collection.Remove(descriptor));
			Assert.IsType<AttributedViewHandlerStub>(factory.GetHandler(typeof(DerivedAttributedViewStub)));
		}

		[Fact]
		public void IndexerReplacementRemovesOldServiceTypeFromResolution()
		{
			using var app = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<AttributedViewStub, AlternateAttributedViewHandlerStub>())
				.Build();
			var factory = app.Services.GetRequiredService<IMauiHandlersFactory>();
			Assert.IsType<AlternateAttributedViewHandlerStub>(factory.GetHandler(typeof(DerivedAttributedViewStub)));
			var collection = factory.GetCollection();
			var descriptor = Assert.Single(collection, item => item.ServiceType == typeof(AttributedViewStub));
			collection[collection.IndexOf(descriptor)] = ServiceDescriptor.Transient(typeof(ButtonStub), typeof(ButtonHandlerStub));

			Assert.IsType<AttributedViewHandlerStub>(factory.GetHandler(typeof(DerivedAttributedViewStub)));
			Assert.IsType<ButtonHandlerStub>(factory.GetHandler(typeof(ButtonStub)));
		}

		[Fact]
		public void CachedRegistrationMissIsInvalidatedByRawCollectionAdd()
		{
			using var app = MauiApp.CreateBuilder().Build();
			var factory = app.Services.GetRequiredService<IMauiHandlersFactory>();
			Assert.IsType<AttributedViewHandlerStub>(factory.GetHandler(typeof(DerivedAttributedViewStub)));

			factory.GetCollection().Add(ServiceDescriptor.Transient(typeof(AttributedViewStub), typeof(AlternateAttributedViewHandlerStub)));

			Assert.IsType<AlternateAttributedViewHandlerStub>(factory.GetHandler(typeof(DerivedAttributedViewStub)));
			Assert.Same(typeof(AlternateAttributedViewHandlerStub), factory.GetHandlerType(typeof(DerivedAttributedViewStub)));
		}

		[Fact]
		public void CustomHandlerCollectionWithoutVersionDoesNotRetainStaleResolution()
		{
			var backing = new Maui.Hosting.Internal.MauiHandlersCollection();
			var collection = Substitute.For<IMauiHandlersCollection>();
			collection.GetEnumerator().Returns(_ => backing.GetEnumerator());
			collection.When(value => value.Add(Arg.Any<ServiceDescriptor>()))
				.Do(args => backing.Add(args.Arg<ServiceDescriptor>()));
			collection.TryGetService(Arg.Any<Type>(), out Arg.Any<ServiceDescriptor>()).Returns(args =>
			{
				var found = backing.TryGetService(args.Arg<Type>(), out var descriptor);
				args[1] = descriptor;
				return found;
			});
			var factory = new Maui.Hosting.Internal.MauiHandlersFactory(collection);
			Assert.IsType<AttributedViewHandlerStub>(factory.GetHandler(typeof(DerivedAttributedViewStub)));

			collection.Add(ServiceDescriptor.Transient(typeof(AttributedViewStub), typeof(AlternateAttributedViewHandlerStub)));

			Assert.IsType<AlternateAttributedViewHandlerStub>(factory.GetHandler(typeof(DerivedAttributedViewStub)));
		}

		[Theory]
		[InlineData("add")]
		[InlineData("insert")]
		[InlineData("replace")]
		public void CachedAssignableRegistrationIsInvalidatedByMoreSpecificRegistration(string mutation)
		{
			using var app = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<AttributedViewStub, AlternateAttributedViewHandlerStub>())
				.Build();
			var factory = app.Services.GetRequiredService<IMauiHandlersFactory>();
			Assert.IsType<AlternateAttributedViewHandlerStub>(factory.GetHandler(typeof(FurtherDerivedAttributedViewStub)));
			var collection = factory.GetCollection();
			var descriptor = ServiceDescriptor.Transient(typeof(DerivedAttributedViewStub), typeof(AttributedViewHandlerStub));
			if (mutation == "add")
				collection.Add(descriptor);
			else if (mutation == "insert")
				collection.Insert(0, descriptor);
			else
				collection[0] = descriptor;

			Assert.IsType<AttributedViewHandlerStub>(factory.GetHandler(typeof(FurtherDerivedAttributedViewStub)));
			Assert.Same(typeof(AttributedViewHandlerStub), factory.GetHandlerType(typeof(FurtherDerivedAttributedViewStub)));
		}

		[Theory]
		[InlineData("remove")]
		[InlineData("remove-at")]
		[InlineData("clear")]
		public void CachedAssignableRegistrationIsInvalidatedByRemoval(string mutation)
		{
			using var app = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<AttributedViewStub, AlternateAttributedViewHandlerStub>();
					handlers.AddHandler<DerivedAttributedViewStub, ViewHandlerStub>();
				})
				.Build();
			var factory = app.Services.GetRequiredService<IMauiHandlersFactory>();
			Assert.IsType<ViewHandlerStub>(factory.GetHandler(typeof(FurtherDerivedAttributedViewStub)));
			var collection = factory.GetCollection();
			var descriptor = Assert.Single(collection, item => item.ServiceType == typeof(DerivedAttributedViewStub));
			if (mutation == "remove")
				Assert.True(collection.Remove(descriptor));
			else if (mutation == "remove-at")
				collection.RemoveAt(collection.IndexOf(descriptor));
			else
				collection.Clear();

			if (mutation == "clear")
				Assert.IsType<AttributedViewHandlerStub>(factory.GetHandler(typeof(FurtherDerivedAttributedViewStub)));
			else
				Assert.IsType<AlternateAttributedViewHandlerStub>(factory.GetHandler(typeof(FurtherDerivedAttributedViewStub)));
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void HotReloadInvalidatesCachedAttributeLookup(bool updateApplication)
		{
			CountingElementHandlerAttribute.Reset();
			using var app = MauiApp.CreateBuilder().Build();
			var factory = app.Services.GetRequiredService<IMauiHandlersFactory>();
			factory.GetHandler(typeof(CountingAttributedViewStub));
			Assert.Equal(1, CountingElementHandlerAttribute.InstanceCount);
			var wasEnabled = MauiHotReloadHelper.IsEnabled;
			try
			{
				if (updateApplication)
					MauiHotReloadHelper.UpdateApplication(Array.Empty<Type>());
				else
					MauiHotReloadHelper.ClearCache(Array.Empty<Type>());
				factory.GetHandler(typeof(CountingAttributedViewStub));
				Assert.Equal(2, CountingElementHandlerAttribute.InstanceCount);
			}
			finally
			{
				MauiHotReloadHelper.IsEnabled = wasEnabled;
				MauiHotReloadHelper.Reset();
			}
		}

		[Fact]
		public void HostBuilderThrowsActionableExceptionForElementHandlerAttributeWithNonHandlerType()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.Build();

			var mauiHandlersFactory = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			var exception = Assert.Throws<HandlerNotFoundException>(() => mauiHandlersFactory.GetHandler(typeof(AttributedNonHandlerViewStub)));

			Assert.Contains(nameof(IElementHandler), exception.Message, StringComparison.Ordinal);
			Assert.Contains(typeof(NonHandler).FullName!, exception.Message, StringComparison.Ordinal);
		}

		[Fact]
		public void HostBuilderThrowsActionableExceptionWhenGettingHandlerTypeForElementHandlerAttributeWithNonHandlerType()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.Build();

			var mauiHandlersFactory = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			var exception = Assert.Throws<HandlerNotFoundException>(() => mauiHandlersFactory.GetHandlerType(typeof(AttributedNonHandlerViewStub)));

			Assert.Contains(nameof(IElementHandler), exception.Message, StringComparison.Ordinal);
			Assert.Contains(typeof(NonHandler).FullName!, exception.Message, StringComparison.Ordinal);
		}

		[Fact]
		public void HostBuilderUsesOverriddenElementHandlerAttributeHandlerType()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.Build();

			var mauiHandlersFactory = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			var handler = mauiHandlersFactory.GetHandler(typeof(OverrideAttributedViewStub));
			var handlerType = mauiHandlersFactory.GetHandlerType(typeof(OverrideAttributedViewStub));

			Assert.IsType<AlternateAttributedViewHandlerStub>(handler);
			Assert.Same(typeof(AlternateAttributedViewHandlerStub), handlerType);
		}

		[Fact]
		public void HostBuilderThrowsForElementHandlerAttributeWithoutHandlerType()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.Build();

			var mauiHandlersFactory = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			Assert.Throws<InvalidOperationException>(() => mauiHandlersFactory.GetHandler(typeof(MissingHandlerTypeAttributedViewStub)));
		}

		[Fact]
		public void HostBuilderCachesElementHandlerAttributeLookup()
		{
			CountingElementHandlerAttribute.Reset();

			var mauiApp = MauiApp.CreateBuilder()
				.Build();

			var mauiHandlersFactory = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			Assert.IsType<AttributedViewHandlerStub>(mauiHandlersFactory.GetHandler(typeof(CountingAttributedViewStub)));
			Assert.IsType<AttributedViewHandlerStub>(mauiHandlersFactory.GetHandler(typeof(CountingAttributedViewStub)));
			Assert.Same(typeof(AttributedViewHandlerStub), mauiHandlersFactory.GetHandlerType(typeof(CountingAttributedViewStub)));
			Assert.Same(typeof(AttributedViewHandlerStub), mauiHandlersFactory.GetHandlerType(typeof(CountingAttributedViewStub)));
			Assert.Equal(1, CountingElementHandlerAttribute.InstanceCount);
		}

		[Fact]
		public void SetVirtualViewRunsControlsMapperRemapOncePerKey()
		{
			RemappableViewStub.Reset();

			new ViewHandlerStub().SetVirtualView(new RemappableViewStub());
			new ViewHandlerStub().SetVirtualView(new DerivedRemappableViewStub());
			new ViewHandlerStub().SetVirtualView(new DerivedRemappableViewStub());

			Assert.Equal(1, RemappableViewStub.RemapCount);
			Assert.Equal(1, RemappableViewStub.DerivedRemapCount);
			Assert.Equal(new[] { "base", "derived" }, RemappableViewStub.RemapOrder);
		}

		[Fact]
		public void DiRegisteredHandlerOverrideStillRunsControlsMapperRemapOnAttach()
		{
			RemappableViewStub.Reset();

			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<AttributedRemappableViewStub, AlternateAttributedViewHandlerStub>();
				})
				.Build();

			var mauiHandlersFactory = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();
			var handler = Assert.IsType<AlternateAttributedViewHandlerStub>(mauiHandlersFactory.GetHandler(typeof(AttributedRemappableViewStub)));

			Assert.Equal(0, RemappableViewStub.RemapCount);

			handler.SetVirtualView(new AttributedRemappableViewStub());

			Assert.Equal(1, RemappableViewStub.RemapCount);
			Assert.Equal(new[] { "base" }, RemappableViewStub.RemapOrder);
		}

		[ElementHandler(typeof(AttributedViewHandlerStub))]
		class AttributedViewStub : ViewStub { }
		class DerivedAttributedViewStub : AttributedViewStub { }
		class FurtherDerivedAttributedViewStub : DerivedAttributedViewStub { }
		class AttributedViewHandlerStub : ViewHandlerStub { }

		[ElementHandler(typeof(AttributedViewHandlerWithoutDefaultConstructorStub))]
		class AttributedViewHandlerWithoutDefaultConstructorViewStub : ViewStub { }
		class AttributedViewHandlerWithoutDefaultConstructorStub : ViewHandlerStub
		{
			public AttributedViewHandlerWithoutDefaultConstructorStub(ConstructorDependency dependency)
			{
				Dependency = dependency;
			}

			public ConstructorDependency Dependency { get; }
		}

		class ConstructorDependency { }

		[ElementHandler(typeof(HandlerWithBothConstructors))]
		class AttributedViewWithBothConstructorsStub : ViewStub { }

		class HandlerWithBothConstructors : ViewHandlerStub
		{
			public HandlerWithBothConstructors() { }

			public HandlerWithBothConstructors(ConstructorDependency dependency)
			{
				UsedInjection = true;
			}

			public bool UsedInjection { get; }
		}

		struct ValueTypeHandler : IElementHandler
		{
			public IMauiContext MauiContext { get; private set; }
			public IElement VirtualView { get; private set; }
			public object PlatformView => null;
			public void SetMauiContext(IMauiContext context) => MauiContext = context;
			public void SetVirtualView(IElement view) => VirtualView = view;
			public void UpdateValue(string property) { }
			public void Invoke(string command, object args = null) { }
			public void DisconnectHandler() { }
		}

		[ElementHandler(typeof(NonHandler))]
		class AttributedNonHandlerViewStub : ViewStub { }
		class NonHandler { }

		[OverrideElementHandler]
		class OverrideAttributedViewStub : ViewStub { }
		class AlternateAttributedViewHandlerStub : ViewHandlerStub { }
		class OverrideElementHandlerAttribute : ElementHandlerAttribute
		{
			public OverrideElementHandlerAttribute()
			{
			}

			public override Type GetHandlerType() => typeof(AlternateAttributedViewHandlerStub);
		}

		[MissingHandlerTypeElementHandler]
		class MissingHandlerTypeAttributedViewStub : ViewStub { }
		class MissingHandlerTypeElementHandlerAttribute : ElementHandlerAttribute { }

		[ElementHandler(typeof(AttributedViewHandlerStub))]
		class AttributedRemappableViewStub : RemappableViewStub { }

		[CountingElementHandler]
		class CountingAttributedViewStub : ViewStub { }
		class CountingElementHandlerAttribute : ElementHandlerAttribute
		{
			public static int InstanceCount { get; private set; }

			public CountingElementHandlerAttribute()
			{
				InstanceCount++;
			}

			public static void Reset() => InstanceCount = 0;

			public override Type GetHandlerType() => typeof(AttributedViewHandlerStub);
		}

		class RemappableViewStub : ViewStub, IControlsMapperRemappable
		{
			static int s_remappedForControls;

			public static int RemapCount { get; private set; }
			public static int DerivedRemapCount { get; protected set; }
			public static List<string> RemapOrder { get; } = new();

			void IControlsMapperRemappable.RemapForControls() => RemapForControls();

			protected virtual void RemapForControls()
			{
				if (Interlocked.CompareExchange(ref s_remappedForControls, 1, 0) != 0)
					return;

				RemapCount++;
				RemapOrder.Add("base");
			}

			public static void Reset()
			{
				s_remappedForControls = 0;
				RemapCount = 0;
				DerivedRemapCount = 0;
				RemapOrder.Clear();
				DerivedRemappableViewStub.ResetDerived();
			}
		}

		class DerivedRemappableViewStub : RemappableViewStub
		{
			static int s_remappedForControls;

			public static void ResetDerived() => s_remappedForControls = 0;

			protected override void RemapForControls()
			{
				if (Interlocked.CompareExchange(ref s_remappedForControls, 1, 0) != 0)
					return;

				base.RemapForControls();
				DerivedRemapCount++;
				RemapOrder.Add("derived");
			}
		}
	}
}
