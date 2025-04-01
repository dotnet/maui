using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class HandlerLifeCycleTests : BaseTestFixture
	{
		[Fact]
		public void BasicVisualTreeDisconnects()
		{
			var mauiApp1 = MauiApp.CreateBuilder()
				.UseMauiApp<ApplicationStub>()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<ContentPage, HandlerStub>())
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<Button, HandlerStub>())
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<VerticalStackLayout, HandlerStub>())
				.Build();


			MauiContext mauiContext1 = new MauiContext(mauiApp1.Services);

			var page = new ContentPage();
			var button1 = new Button();
			var button2 = new Button();
			var layout1 = new VerticalStackLayout()
			{
				button1,
				button2
			};

			page.Content = layout1;

			page.ToHandler(mauiContext1);
			button1.ToHandler(mauiContext1);
			button2.ToHandler(mauiContext1);
			layout1.ToHandler(mauiContext1);

			page.HandlerChanged += (_, __) =>
			{
				Assert.NotNull(button1.Handler);
				Assert.NotNull(button2.Handler);
				Assert.NotNull(layout1.Handler);

			};

			layout1.HandlerChanged += (_, __) =>
			{
				Assert.Null(page.Handler);
				Assert.NotNull(button1.Handler);
				Assert.NotNull(button2.Handler);
			};

			Assert.NotNull(page.Handler);
			Assert.NotNull(button1.Handler);
			Assert.NotNull(button2.Handler);
			Assert.NotNull(layout1.Handler);


			page.DisconnectHandlers();

			Assert.Null(page.Handler);
			Assert.Null(button1.Handler);
			Assert.Null(button2.Handler);
			Assert.Null(layout1.Handler);
		}

		[Fact]
		public void InterruptDisconnect()
		{
			var mauiApp1 = MauiApp.CreateBuilder()
				.UseMauiApp<ApplicationStub>()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<ContentPage, HandlerStub>())
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<Button, HandlerStub>())
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<VerticalStackLayout, HandlerStub>())
				.Build();


			MauiContext mauiContext1 = new MauiContext(mauiApp1.Services);

			var page = new ContentPage();
			var button1 = new Button();
			var button2 = new Button();
			var layout1 = new VerticalStackLayout()
			{
				button1,
				button2
			};

			var layout2 = new VerticalStackLayout()
			{
				 new Button(),
				 new Button()
			};

			page.Content = new VerticalStackLayout()
			{
				layout1, layout2
			};

			HandlerProperties.SetDisconnectPolicy(layout2, HandlerDisconnectPolicy.Manual);

			page.ToHandler(mauiContext1);
			button1.ToHandler(mauiContext1);
			button2.ToHandler(mauiContext1);
			layout1.ToHandler(mauiContext1);
			layout2.ToHandler(mauiContext1);
			layout2[0].ToHandler(mauiContext1);
			layout2[1].ToHandler(mauiContext1);

			Assert.NotNull(layout2.Handler);
			Assert.NotNull(layout1.Handler);

			page.DisconnectHandlers();

			Assert.NotNull(layout2.Handler);
			Assert.NotNull(layout2[0].Handler);
			Assert.NotNull(layout2[1].Handler);
			Assert.Null(layout1.Handler);
		}


		[Fact]
		public void SettingHandlerToNullDisconnectsHandlerFromVirtualView()
		{
			var mauiApp1 = MauiApp.CreateBuilder()
				.UseMauiApp<ApplicationStub>()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<LifeCycleButton, HandlerStub>())
				.Build();

			LifeCycleButton button = new LifeCycleButton();

			MauiContext mauiContext1 = new MauiContext(mauiApp1.Services);
			var handler1 = button.ToHandler(mauiContext1);
			button.Handler = null;
			Assert.NotEqual(button, handler1.VirtualView);
		}

		[Fact]
		public void SettingNewHandlerDisconnectsOldHandler()
		{
			var mauiApp1 = MauiApp.CreateBuilder()
				.UseMauiApp<ApplicationStub>()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<LifeCycleButton, HandlerStub>())
				.Build();

			LifeCycleButton button = new LifeCycleButton();

			MauiContext mauiContext1 = new MauiContext(mauiApp1.Services);
			var handler1 = button.ToHandler(mauiContext1);

			var mauiApp2 = MauiApp.CreateBuilder()
				.UseMauiApp<ApplicationStub>()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<LifeCycleButton, HandlerStub>())
				.Build();

			MauiContext mauiContext2 = new MauiContext(mauiApp2.Services);

			List<HandlerChangingEventArgs> changingArgs = new List<HandlerChangingEventArgs>();
			button.HandlerChanging += (s, a) =>
			{
				Assert.Equal(handler1, a.OldHandler);
				Assert.NotNull(a.NewHandler);

				changingArgs.Add(a);
			};

			var handler2 = button.ToHandler(mauiContext2);

			Assert.NotEqual(button, handler1.VirtualView);
			Assert.Equal(button, handler2.VirtualView);
			Assert.Single(changingArgs);
		}

		[Fact]
		public void ChangingAndChangedBothFireInitially()
		{
			LifeCycleButton button = new LifeCycleButton();
			bool changing = false;
			bool changed = false;

			button.HandlerChanging += (_, __) => changing = true;
			button.HandlerChanged += (_, __) =>
			{
				if (!changing)
					throw new XunitException("Attached fired before changing");

				changed = true;
			};

			Assert.Equal(0, button.changing);
			Assert.Equal(0, button.changed);
			Assert.False(changing);
			Assert.False(changed);

			button.Handler = new HandlerStub();

			Assert.True(changing);
			Assert.True(changed);
			Assert.Equal(1, button.changing);
			Assert.Equal(1, button.changed);
		}

		[Fact]
		public void ChangingArgsAreSetCorrectly()
		{
			LifeCycleButton button = new LifeCycleButton();

			Assert.Null(button.Handler);
			var firstHandler = new HandlerStub();
			button.Handler = firstHandler;

			Assert.Equal(button.LastHandlerChangingEventArgs.NewHandler, firstHandler);
			Assert.Null(button.LastHandlerChangingEventArgs.OldHandler);

			var secondHandler = new HandlerStub();
			button.Handler = secondHandler;
			Assert.Equal(button.LastHandlerChangingEventArgs.OldHandler, firstHandler);
			Assert.Equal(button.LastHandlerChangingEventArgs.NewHandler, secondHandler);

			button.Handler = null;
			Assert.Equal(button.LastHandlerChangingEventArgs.OldHandler, secondHandler);
			Assert.Null(button.LastHandlerChangingEventArgs.NewHandler);

			Assert.Equal(3, button.changing);
			Assert.Equal(3, button.changed);
		}

		public class LifeCycleButton : Button
		{
			public int changing = 0;
			public int changed = 0;
			public HandlerChangingEventArgs LastHandlerChangingEventArgs { get; set; }

			protected override void OnHandlerChanging(HandlerChangingEventArgs args)
			{
				LastHandlerChangingEventArgs = args;
				changing++;
				base.OnHandlerChanging(args);
			}

			protected override void OnHandlerChanged()
			{
				changed++;
				base.OnHandlerChanged();

				if (changed != changing)
					throw new XunitException("Attaching/Attached fire mismatch");
			}
		}
	}
}
