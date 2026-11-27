using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
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
		public void ToHandlerUsesActivatorUtilitiesForHandlerWithoutDefaultConstructor()
		{
			var dependency = new ConstructorDependency();
			var builder = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<ViewStub, HandlerWithoutDefaultConstructorStub>());
			builder.Services.AddSingleton(dependency);
			using var mauiApp = builder.Build();
			var mauiContext = new MauiContext(mauiApp.Services);

			var handler = Assert.IsType<HandlerWithoutDefaultConstructorStub>(new ViewStub().ToHandler(mauiContext));

			Assert.Same(dependency, handler.Dependency);
		}

		[Fact]
		public void ConstructorInjectionUsesCurrentContextAndHonorsFactoryOverrides()
		{
			using var app = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<ViewStub, HandlerWithoutDefaultConstructorStub>())
				.Build();
			var firstContext = new MauiContext(app.Services);
			var secondContext = new MauiContext(app.Services);
			var firstDependency = new ConstructorDependency();
			var secondDependency = new ConstructorDependency();
			firstContext.AddSpecific(firstDependency);
			secondContext.AddSpecific(secondDependency);

			Assert.Same(firstDependency, Assert.IsType<HandlerWithoutDefaultConstructorStub>(
				new ViewStub().ToHandler(firstContext)).Dependency);
			Assert.Same(secondDependency, Assert.IsType<HandlerWithoutDefaultConstructorStub>(
				new ViewStub().ToHandler(secondContext)).Dependency);

			var calls = 0;
			firstContext.Handlers.GetCollection().AddHandler<ViewStub>(_ =>
			{
				calls++;
				return new AlternateViewHandlerStub();
			});
			Assert.IsType<AlternateViewHandlerStub>(new ViewStub().ToHandler(firstContext));
			Assert.IsType<AlternateViewHandlerStub>(new ViewStub().ToHandler(secondContext));
			Assert.Equal(2, calls);
		}

		[Fact]
		public void ConstructorInjectionFailureIsNotRetried()
		{
			var calls = 0;
			var builder = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<ViewStub, HandlerWithoutDefaultConstructorStub>());
			builder.Services.AddTransient<ConstructorDependency>(_ =>
			{
				calls++;
				throw new MissingMethodException("Dependency factory failed.");
			});
			using var app = builder.Build();
			var context = new MauiContext(app.Services);

			Assert.Throws<MissingMethodException>(() => new ViewStub().ToHandler(context));
			Assert.Equal(1, calls);

			context.AddSpecific(new ConstructorDependency());
			Assert.IsType<HandlerWithoutDefaultConstructorStub>(new ViewStub().ToHandler(context));
		}

		[Fact]
		public void ToHandlerPrefersParameterlessConstructorWhenInjectionIsAlsoAvailable()
		{
			var builder = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<ViewStub, HandlerWithBothConstructors>());
			builder.Services.AddTransient<ConstructorDependency>(_ =>
				throw new InvalidOperationException("The parameterless constructor must not resolve dependencies."));
			using var app = builder.Build();
			var context = new MauiContext(app.Services);

			var handler = Assert.IsType<HandlerWithBothConstructors>(new ViewStub().ToHandler(context));

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

		[Fact]
		public void ConstructorInjectionAvoidsMissingConstructorExceptions()
		{
			var builder = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<ViewStub, HandlerWithoutDefaultConstructorStub>());
			builder.Services.AddSingleton(new ConstructorDependency());
			using var app = builder.Build();
			var context = new MauiContext(app.Services);
			var exceptions = 0;
			var threadId = Environment.CurrentManagedThreadId;
			EventHandler<FirstChanceExceptionEventArgs> onException = (_, args) =>
			{
				if (Environment.CurrentManagedThreadId == threadId && args.Exception is MissingMethodException)
					exceptions++;
			};

			AppDomain.CurrentDomain.FirstChanceException += onException;
			try
			{
				Assert.IsType<HandlerWithoutDefaultConstructorStub>(new ViewStub().ToHandler(context));
				Assert.Equal(0, exceptions);
				Assert.IsType<HandlerWithoutDefaultConstructorStub>(new ViewStub().ToHandler(context));
				Assert.Equal(0, exceptions);

				context.Handlers.GetCollection().AddHandler<ButtonStub>(_ => new ButtonHandlerStub());
				Assert.IsType<HandlerWithoutDefaultConstructorStub>(new ViewStub().ToHandler(context));
				Assert.Equal(0, exceptions);

				MauiHotReloadHelper.ClearCache(Array.Empty<Type>());
				Assert.IsType<HandlerWithoutDefaultConstructorStub>(new ViewStub().ToHandler(context));
				Assert.Equal(0, exceptions);
			}
			finally
			{
				AppDomain.CurrentDomain.FirstChanceException -= onException;
				MauiHotReloadHelper.Reset();
			}

			Assert.Throws<MissingMethodException>(() => context.Handlers.GetHandler(typeof(ViewStub)));
		}

		[Fact]
		public void ConstructorInjectionDoesNotAffectOtherApps()
		{
			var builder = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<ViewStub, HandlerWithoutDefaultConstructorStub>());
			builder.Services.AddSingleton(new ConstructorDependency());
			using var firstApp = builder.Build();
			new ViewStub().ToHandler(new MauiContext(firstApp.Services));

			using var secondApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<ViewStub>(_ => new AlternateViewHandlerStub()))
				.Build();
			Assert.IsType<AlternateViewHandlerStub>(new ViewStub().ToHandler(new MauiContext(secondApp.Services)));
		}

		[Fact]
		public void HotReloadReplacesHandlerAfterConstructorInjection()
		{
			var builder = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<ViewStub, HandlerWithoutDefaultConstructorStub>());
			builder.Services.AddSingleton(new ConstructorDependency());
			using var app = builder.Build();
			var context = new MauiContext(app.Services);
			MauiHotReloadHelper.RegisterHandlers(context.Handlers.GetCollection());
			try
			{
				Assert.IsType<HandlerWithoutDefaultConstructorStub>(new ViewStub().ToHandler(context));
				Assert.IsType<HandlerWithoutDefaultConstructorStub>(new ViewStub().ToHandler(context));

				MauiHotReloadHelper.RegisterHandlerReplacement(
					typeof(HandlerWithoutDefaultConstructorStub).FullName!,
					typeof(AlternateViewHandlerStub));

				Assert.IsType<AlternateViewHandlerStub>(new ViewStub().ToHandler(context));
			}
			finally
			{
				MauiHotReloadHelper.Reset();
			}
		}

		[Fact]
		public void WrappedMissingConstructorFromInjectedDependencyIsNotRetried()
		{
			var calls = 0;
			var exception = new HandlerNotFoundException("Wrapped.", new MissingMethodException());
			var builder = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<ViewStub, HandlerWithoutDefaultConstructorStub>());
			builder.Services.AddTransient<ConstructorDependency>(_ =>
			{
				calls++;
				throw exception;
			});
			using var app = builder.Build();

			Assert.Same(exception, Assert.Throws<HandlerNotFoundException>(() =>
				new ViewStub().ToHandler(new MauiContext(app.Services))));
			Assert.Equal(1, calls);
		}

		[Fact]
		public void SingletonRegistrationRequiringInjectionCreatesHandlerPerActivation()
		{
			using var app = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<ViewStub, HandlerWithoutDefaultConstructorStub>();
					handlers.Add(ServiceDescriptor.Singleton(typeof(ViewStub), typeof(HandlerWithoutDefaultConstructorStub)));
				})
				.Build();
			var firstContext = new MauiContext(app.Services);
			var secondContext = new MauiContext(app.Services);
			var firstDependency = new ConstructorDependency();
			var secondDependency = new ConstructorDependency();
			firstContext.AddSpecific(firstDependency);
			secondContext.AddSpecific(secondDependency);

			var firstView = new ViewStub();
			var secondView = new ViewStub();
			var first = Assert.IsType<HandlerWithoutDefaultConstructorStub>(firstView.ToHandler(firstContext));
			var second = Assert.IsType<HandlerWithoutDefaultConstructorStub>(secondView.ToHandler(firstContext));
			var other = Assert.IsType<HandlerWithoutDefaultConstructorStub>(new ViewStub().ToHandler(secondContext));

			Assert.NotSame(first, second);
			Assert.Same(firstView, first.VirtualView);
			Assert.Same(secondView, second.VirtualView);
			Assert.Same(firstDependency, first.Dependency);
			Assert.Same(firstDependency, second.Dependency);
			Assert.Same(secondDependency, other.Dependency);
		}

		[Fact]
		public void CustomHandlersFactoryWrappedMissingConstructorFallsBackToInjection()
		{
			var handlers = Substitute.For<IMauiHandlersFactory>();
			handlers.GetHandler(typeof(ViewStub)).Returns(_ =>
				throw new HandlerNotFoundException("Wrapped.", new MissingMethodException()));
			handlers.GetHandlerType(typeof(ViewStub)).Returns(typeof(HandlerWithoutDefaultConstructorStub));
			var dependency = new ConstructorDependency();
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton(handlers);
			builder.Services.AddSingleton(dependency);
			using var app = builder.Build();

			var handler = Assert.IsType<HandlerWithoutDefaultConstructorStub>(
				new ViewStub().ToHandler(new MauiContext(app.Services)));

			Assert.Same(dependency, handler.Dependency);
		}

		[Fact]
		public void CustomHandlersFactoryWrappedUnrelatedExceptionPropagates()
		{
			var handlers = Substitute.For<IMauiHandlersFactory>();
			var exception = new HandlerNotFoundException("Wrapped.", new InvalidOperationException());
			handlers.GetHandler(typeof(ViewStub)).Returns(_ => throw exception);
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton(handlers);
			using var app = builder.Build();

			Assert.Same(exception, Assert.Throws<HandlerNotFoundException>(() =>
				new ViewStub().ToHandler(new MauiContext(app.Services))));
			handlers.DidNotReceive().GetHandlerType(Arg.Any<Type>());
		}

		class HandlerWithoutDefaultConstructorStub : ViewHandlerStub
		{
			public HandlerWithoutDefaultConstructorStub(ConstructorDependency dependency)
			{
				Dependency = dependency;
			}

			public ConstructorDependency Dependency { get; }
		}

		class ConstructorDependency { }

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
	}
}
