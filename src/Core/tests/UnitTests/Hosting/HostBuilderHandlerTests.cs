using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Handlers;
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
		public void HostBuilderPrefersRegisteredInterfaceHandlerOverElementHandlerAttribute()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<IViewStub, AlternateAttributedViewHandlerStub>())
				.Build();

			var mauiHandlersFactory = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			var handler = mauiHandlersFactory.GetHandler(typeof(AttributedViewStub));
			var handlerType = mauiHandlersFactory.GetHandlerType(typeof(AttributedViewStub));

			Assert.IsType<AlternateAttributedViewHandlerStub>(handler);
			Assert.Same(typeof(AlternateAttributedViewHandlerStub), handlerType);
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
			Assert.Contains(nameof(MauiHandlersCollectionExtensions.AddHandler), exception.Message, StringComparison.Ordinal);
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

		[ElementHandler(typeof(AttributedViewHandlerStub))]
		class AttributedViewStub : ViewStub { }
		class DerivedAttributedViewStub : AttributedViewStub { }
		class AttributedViewHandlerStub : ViewHandlerStub { }

		[ElementHandler(typeof(AttributedViewHandlerWithoutDefaultConstructorStub))]
		class AttributedViewHandlerWithoutDefaultConstructorViewStub : ViewStub { }
		class AttributedViewHandlerWithoutDefaultConstructorStub : ViewHandlerStub
		{
			public AttributedViewHandlerWithoutDefaultConstructorStub(string value)
			{
			}
		}

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
