using Android.Content;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.MauiContext)]
	public class HandlerActivationTests
	{
		[Theory(DisplayName = "Injected Handler Captures The Current Android Context")]
		[InlineData(false)]
		[InlineData(true)]
		public void InjectedHandlerCapturesCurrentAndroidContextAndDependency(bool singleton)
		{
			using var app = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<ContextElementStub, ContextCapturingHandler>();
					if (singleton)
						handlers.Add(ServiceDescriptor.Singleton(typeof(ContextElementStub), typeof(ContextCapturingHandler)));
				})
				.Build();
			var firstAndroidContext = new ContextWrapper(MauiProgram.DefaultContext);
			var secondAndroidContext = new ContextWrapper(MauiProgram.DefaultContext);
			var firstContext = new MauiContext(app.Services, firstAndroidContext);
			var secondContext = new MauiContext(app.Services, secondAndroidContext);
			var firstDependency = new ContextDependency();
			var secondDependency = new ContextDependency();
			firstContext.AddSpecific(firstDependency);
			secondContext.AddSpecific(secondDependency);

			var first = Assert.IsType<ContextCapturingHandler>(new ContextElementStub().ToHandler(firstContext));
			var again = Assert.IsType<ContextCapturingHandler>(new ContextElementStub().ToHandler(firstContext));
			var second = Assert.IsType<ContextCapturingHandler>(new ContextElementStub().ToHandler(secondContext));

			Assert.NotSame(first, again);
			Assert.Same(firstAndroidContext, first.AndroidContext);
			Assert.Same(firstDependency, first.Dependency);
			Assert.Same(firstAndroidContext, again.AndroidContext);
			Assert.Same(secondAndroidContext, second.AndroidContext);
			Assert.Same(secondDependency, second.Dependency);
		}

		class ContextElementStub : ElementStub
		{
		}

		class ContextDependency
		{
		}

		class ContextCapturingHandler : ElementHandler<ContextElementStub, object>
		{
			public ContextCapturingHandler(Context androidContext, ContextDependency dependency)
				: base(ElementHandler.ElementMapper)
			{
				AndroidContext = androidContext;
				Dependency = dependency;
			}

			public Context AndroidContext { get; }

			public ContextDependency Dependency { get; }

			protected override object CreatePlatformElement() => new object();
		}
	}
}
