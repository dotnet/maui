using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Hosting.Internal;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	[Category(TestCategory.Core, TestCategory.Lifecycle)]
	public class AbstractViewHandlerTests
	{
		[Fact]
		public void ConnectAndDisconnectFireAppropriateNumberOfTimes()
		{
			HandlerStub handlerStub = new HandlerStub();
			handlerStub.SetVirtualView(new Maui.Controls.Button());

			Assert.Equal(1, handlerStub.ConnectHandlerCount);
			Assert.Equal(0, handlerStub.DisconnectHandlerCount);

			handlerStub.SetVirtualView(new Maui.Controls.Button());
			handlerStub.SetVirtualView(new Maui.Controls.Button());
			handlerStub.SetVirtualView(new Maui.Controls.Button());
			Assert.Equal(1, handlerStub.ConnectHandlerCount);
			Assert.Equal(0, handlerStub.DisconnectHandlerCount);

			(handlerStub as IViewHandler).DisconnectHandler();
			Assert.Equal(1, handlerStub.ConnectHandlerCount);
			Assert.Equal(1, handlerStub.DisconnectHandlerCount);

			(handlerStub as IViewHandler).DisconnectHandler();
			Assert.Equal(1, handlerStub.ConnectHandlerCount);
			Assert.Equal(1, handlerStub.DisconnectHandlerCount);


			handlerStub.SetVirtualView(new Maui.Controls.Button());
			Assert.Equal(2, handlerStub.ConnectHandlerCount);
			Assert.Equal(1, handlerStub.DisconnectHandlerCount);
			(handlerStub as IViewHandler).DisconnectHandler();
			Assert.Equal(2, handlerStub.ConnectHandlerCount);
			Assert.Equal(2, handlerStub.DisconnectHandlerCount);
		}

		[Fact]
		public void GetRequiredServiceThrowsOnNoContext()
		{
			HandlerStub handlerStub = new HandlerStub();

			Assert.Null((handlerStub as IViewHandler).MauiContext);

			var ex = Assert.Throws<InvalidOperationException>(() => handlerStub.GetRequiredService<IFooService>());

			Assert.Contains("the context", ex.Message, StringComparison.Ordinal);
			Assert.Contains("MauiContext", ex.Message, StringComparison.Ordinal);
		}

		[Fact]
		public void GetRequiredServiceThrowsOnNoServices()
		{
			HandlerStub handlerStub = new HandlerStub();

			handlerStub.SetMauiContext(new InvalidHandlersContextStub());

			Assert.NotNull(handlerStub.MauiContext);
			Assert.Null(handlerStub.MauiContext.Services);

			var ex = Assert.Throws<InvalidOperationException>(() => handlerStub.GetRequiredService<IFooService>());

			Assert.Contains("the service provider", ex.Message, StringComparison.Ordinal);
			Assert.Contains("MauiContext", ex.Message, StringComparison.Ordinal);
		}

		[Fact]
		public void GetRequiredServiceRetrievesService()
		{
			HandlerStub handlerStub = new HandlerStub();

			var collection = new MauiHandlersCollection();
			collection.TryAddSingleton<IMauiHandlersFactory>(new MauiHandlersFactory(collection));
			collection.TryAddSingleton<IFooService, FooService>();

			var provider = new MauiFactory(collection);

			handlerStub.SetMauiContext(new HandlersContextStub(provider));

			Assert.NotNull(handlerStub.MauiContext);
			Assert.NotNull(handlerStub.MauiContext.Services);

			var foo = handlerStub.GetRequiredService<IFooService>();

			Assert.IsType<FooService>(foo);
		}

		[Fact]
		public void SettingVirtualViewOnHandlerRemovesHandlerFromPreviousVirtualView()
		{
			HandlerStub handlerStub = new HandlerStub();
			var button1 = new Maui.Controls.Button();
			var button2 = new Maui.Controls.Button();
			handlerStub.SetVirtualView(button1);
			handlerStub.SetVirtualView(button2);

			Assert.Null(button1.Handler);
		}

		[Fact]
		public void ChainingToLessTypedParentWorks()
		{
			bool wasMapper1Called = false;
			bool wasMapper2Called = false;
			var mapper1 = new PropertyMapper<IView, HandlerStub>
			{
				[nameof(IView.Background)] = (r, v) => wasMapper1Called = true
			};


			var mapper2 = new PropertyMapper<Button, HandlerStub>(mapper1)
			{
				[nameof(IView.Background)] = (r, v) => wasMapper2Called = true
			};

			HandlerStub handlerStub = new HandlerStub(mapper2);
			handlerStub.SetVirtualView(new ButtonStub());
			handlerStub.UpdateValue(nameof(IView.Background));

			Assert.True(wasMapper1Called);
			Assert.False(wasMapper2Called);
		}

		class CustomNativeButton : object
		{

		}

		class CustomButton : Maui.Controls.Button
		{

		}

		[Fact]
		public void CanUseFactoryForAlternateType()
		{
			HandlerStub.PlatformViewFactory = (h) => { return new CustomNativeButton(); };

			HandlerStub handlerStub = new HandlerStub();
			handlerStub.SetVirtualView(new Maui.Controls.Button());

			Assert.True(handlerStub.PlatformView is CustomNativeButton);
		}

		[Fact]
		public void FactoryCanPuntAndUseOriginalType()
		{
			HandlerStub.PlatformViewFactory = (h) => { return null; };

			HandlerStub handlerStub = new HandlerStub();
			handlerStub.SetVirtualView(new Maui.Controls.Button());

			Assert.NotNull(handlerStub.PlatformView);
			Assert.False(handlerStub.PlatformView is CustomNativeButton);
			Assert.True(handlerStub.PlatformView is object);
		}

		[Fact]
		public void FactoryCanCustomizeBasedOnVirtualView()
		{
			HandlerStub.PlatformViewFactory = (h) =>
			{
				if (h.VirtualView is CustomButton)
				{
					return new CustomNativeButton();
				}

				return null;
			};

			HandlerStub handlerStub = new HandlerStub();
			handlerStub.SetVirtualView(new CustomButton());

			Assert.True(handlerStub.PlatformView is CustomNativeButton);

			HandlerStub handlerStub2 = new HandlerStub();
			handlerStub2.SetVirtualView(new Maui.Controls.Button());

			Assert.True(handlerStub2.PlatformView is object);
		}
	}
}