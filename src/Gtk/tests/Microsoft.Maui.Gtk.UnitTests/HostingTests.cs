using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Animations;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Gtk.Dispatching;
using Microsoft.Maui.Gtk.Fonts;
using Microsoft.Maui.Gtk.Handlers;
using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.Gtk.UnitTests
{
	public class HostingTests
	{
		[GtkFact]
		public void StockHandlerRegistrationsAreRewrittenToGtkHandlers() => GtkTestHost.Run(() =>
		{
			var factory = GtkTestApp.App.Services.GetRequiredService<IMauiHandlersFactory>();

			Assert.Equal(typeof(GtkLabelHandler), factory.GetHandlerType(typeof(Label)));
			Assert.Equal(typeof(GtkButtonHandler), factory.GetHandlerType(typeof(Microsoft.Maui.Controls.Button)));
			Assert.Equal(typeof(GtkEntryHandler), factory.GetHandlerType(typeof(Entry)));
			Assert.Equal(typeof(GtkEditorHandler), factory.GetHandlerType(typeof(Editor)));
			Assert.Equal(typeof(GtkImageHandler), factory.GetHandlerType(typeof(Microsoft.Maui.Controls.Image)));
			Assert.Equal(typeof(GtkScrollViewHandler), factory.GetHandlerType(typeof(ScrollView)));
			Assert.Equal(typeof(GtkBorderHandler), factory.GetHandlerType(typeof(Border)));
			Assert.Equal(typeof(GtkSwitchHandler), factory.GetHandlerType(typeof(Microsoft.Maui.Controls.Switch)));
			Assert.Equal(typeof(GtkCheckBoxHandler), factory.GetHandlerType(typeof(CheckBox)));
			Assert.Equal(typeof(GtkLayoutHandler), factory.GetHandlerType(typeof(VerticalStackLayout)));
			Assert.Equal(typeof(GtkLayoutHandler), factory.GetHandlerType(typeof(Grid)));
			Assert.Equal(typeof(GtkContentViewHandler), factory.GetHandlerType(typeof(ContentView)));
			Assert.Equal(typeof(GtkPageHandler), factory.GetHandlerType(typeof(ContentPage)));
			Assert.Equal(typeof(GtkWindowHandler), factory.GetHandlerType(typeof(Microsoft.Maui.Controls.Window)));
			Assert.Equal(typeof(GtkApplicationHandler), factory.GetHandlerType(typeof(TestApplication)));
		});

		[GtkFact]
		public void HandlerMapRejectsTypesThatDoNotDeriveFromTheStockHandler()
		{
			Assert.Throws<ArgumentException>(() => GtkHandlerMap.Add(typeof(LabelHandler), typeof(GtkButtonHandler)));
			Assert.Throws<ArgumentNullException>(() => GtkHandlerMap.Add(null!, typeof(GtkLabelHandler)));
		}

		[GtkFact]
		public void HandlerMapAcceptsExtraEntriesFromOtherAssemblies()
		{
			Assert.True(GtkHandlerMap.TryGetGtkHandler(typeof(LabelHandler), out var gtk));
			Assert.Equal(typeof(GtkLabelHandler), gtk);
			Assert.True(GtkHandlerMap.TryGetGtkHandler(typeof(SliderHandler), out var slider));
			Assert.Equal(typeof(GtkSliderHandler), slider);
			Assert.False(GtkHandlerMap.TryGetGtkHandler(typeof(string), out _));
		}

		[GtkFact]
		public void DispatcherIsInlineOnTheGtkThreadAndNullElsewhere() => GtkTestHost.Run(() =>
		{
			var provider = GtkTestApp.App.Services.GetRequiredService<IDispatcherProvider>();
			Assert.IsType<GtkDispatcherProvider>(provider);

			var dispatcher = provider.GetForCurrentThread();
			Assert.NotNull(dispatcher);
			Assert.False(dispatcher!.IsDispatchRequired);

			int order = 0;
			int ran = -1;
			Assert.True(dispatcher.Dispatch(() => ran = order++));
			Assert.Equal(0, ran);

			var other = System.Threading.Tasks.Task.Run(() => provider.GetForCurrentThread());
			Assert.Null(GtkTestHost.Await(other));
		});

		[GtkFact]
		public void DispatchDelayedAndTimerRunOnTheMainLoop() => GtkTestHost.Run(() =>
		{
			var dispatcher = new GtkDispatcher();
			bool delayed = false;
			dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(10), () => delayed = true);
			Assert.True(GtkTestHost.PumpUntil(() => delayed));

			var timer = dispatcher.CreateTimer();
			timer.Interval = TimeSpan.FromMilliseconds(5);
			timer.IsRepeating = true;
			int ticks = 0;
			timer.Tick += (_, _) => ticks++;
			timer.Start();
			Assert.True(timer.IsRunning);
			Assert.True(GtkTestHost.PumpUntil(() => ticks >= 3));
			timer.Stop();
			Assert.False(timer.IsRunning);

			int stopped = ticks;
			GtkTestHost.Pump(null, 3);
			System.Threading.Thread.Sleep(30);
			GtkTestHost.Pump(null, 3);
			Assert.Equal(stopped, ticks);
		});

		[GtkFact]
		public void DispatcherFromOtherThreadsQueuesOntoTheMainLoop() => GtkTestHost.Run(() =>
		{
			var dispatcher = new GtkDispatcher();
			bool ran = false;
			int threadId = -1;
			int gtkThread = Environment.CurrentManagedThreadId;

			var task = System.Threading.Tasks.Task.Run(() =>
			{
				Assert.True(dispatcher.IsDispatchRequired);
				dispatcher.Dispatch(() =>
				{
					ran = true;
					threadId = Environment.CurrentManagedThreadId;
				});
			});

			GtkTestHost.Await(task);
			Assert.True(GtkTestHost.PumpUntil(() => ran));
			Assert.Equal(gtkThread, threadId);
		});

		[GtkFact]
		public void TickerFiresWithoutAWindowAndStops() => GtkTestHost.Run(() =>
		{
			using var scope = GtkTestApp.App.Services.CreateScope();
			var ticker = scope.ServiceProvider.GetRequiredService<ITicker>();
			Assert.IsType<GtkTicker>(ticker);

			int fired = 0;
			ticker.Fire = () => fired++;
			ticker.Start();
			Assert.True(ticker.IsRunning);
			Assert.True(GtkTestHost.PumpUntil(() => fired >= 2));

			ticker.Stop();
			Assert.False(ticker.IsRunning);
		});

		[GtkFact]
		public void TickerUsesTheFrameClockOfAMappedWindow() => GtkTestHost.Run(() =>
		{
			var window = new global::Gtk.Window();
			window.SetDefaultSize(100, 100);
			window.Present();
			Assert.True(GtkTestHost.PumpUntil(() => window.IsMapped, window));

			var ticker = new GtkTicker { MaxFps = 60 };
			int fired = 0;
			ticker.Fire = () => fired++;
			ticker.Start();

			bool fired2 = GtkTestHost.PumpUntil(() => fired >= 1, window, 3000);
			ticker.Stop();
			GtkTestHost.Retire(window);

			Assert.True(fired2, "no frame clock tick arrived");
		});

		[GtkFact]
		public void TickerReanchorsWhenItsWindowIsUnmapped() => GtkTestHost.Run(() =>
		{
			var first = new global::Gtk.Window();
			first.SetDefaultSize(100, 100);
			first.Present();
			Assert.True(GtkTestHost.PumpUntil(() => first.IsMapped, first));

			var ticker = new GtkTicker { MaxFps = 60 };
			int fired = 0;
			ticker.Fire = () => fired++;
			ticker.Start();
			Assert.True(GtkTestHost.PumpUntil(() => fired >= 1, first, 3000));

			// A second window exists; the anchor window goes away: ticks must continue.
			var second = new global::Gtk.Window();
			second.SetDefaultSize(100, 100);
			second.Present();
			Assert.True(GtkTestHost.PumpUntil(() => second.IsMapped, second));

			first.Visible = false;
			Assert.True(GtkTestHost.PumpUntil(() => !first.IsMapped, second));
			int before = fired;
			bool resumed = GtkTestHost.PumpUntil(() => fired >= before + 3, second, 3000);

			ticker.Stop();
			GtkTestHost.Retire(first);
			GtkTestHost.Retire(second);

			Assert.True(resumed, "ticks stopped after the anchor window was unmapped");
		});

		[GtkFact]
		public void HandlerRewriteDoesNotDependOnTheCallOrder() => GtkTestHost.Run(() =>
		{
			// UseMauiGtkCore before UseMauiApp (stock handlers are registered afterwards) and a later ConfigureMauiHandlers.
			var builder = MauiApp.CreateBuilder();
			builder.UseMauiGtkCore().UseMauiApp<TestApplication>();
			builder.ConfigureMauiHandlers(h => h.AddHandler<Editor, EditorHandler>());
			var app = builder.Build();

			var factory = app.Services.GetRequiredService<IMauiHandlersFactory>();
			Assert.Equal(typeof(GtkLabelHandler), factory.GetHandlerType(typeof(Label)));
			Assert.Equal(typeof(GtkButtonHandler), factory.GetHandlerType(typeof(Microsoft.Maui.Controls.Button)));
			Assert.Equal(typeof(GtkEditorHandler), factory.GetHandlerType(typeof(Editor)));
		});

		[GtkFact]
		public void ARegistrationMoreSpecificThanTheRewrittenStockOneWinsInAnyOrder()
		{
			foreach (bool specificFirst in new[] { true, false })
			{
				var handlers = new HandlerMauiAppBuilderExtensions.HandlerServiceBuilder(null!);
				if (specificFirst)
					handlers.AddHandler<Label, DerivedGtkLabelHandler>();
				handlers.AddHandler<Label, LabelHandler>();
				if (!specificFirst)
					handlers.AddHandler<Label, DerivedGtkLabelHandler>();

				GtkHandlerMap.Apply(handlers);

				System.Type? winner = null;
				foreach (var d in handlers)
					if (d.ServiceType == typeof(Label))
						winner = d.ImplementationType;
				Assert.Equal(typeof(DerivedGtkLabelHandler), winner);
			}
		}

		sealed class DerivedGtkLabelHandler : GtkLabelHandler
		{
		}

		[GtkFact]
		public void FontManagerAndEmbeddedLoaderAreRegistered() => GtkTestHost.Run(() =>
		{
			Assert.IsType<GtkFontManager>(GtkTestApp.App.Services.GetRequiredService<IFontManager>());
			Assert.IsType<GtkEmbeddedFontLoader>(GtkTestApp.App.Services.GetRequiredService<IEmbeddedFontLoader>());
		});

		[GtkFact]
		public void StockMapperRemapsAndCustomisationsStillRunAfterTheGtkMapping() => GtkTestHost.Run(() =>
		{
			var app = GtkTestApp.App;
			var ran = new System.Collections.Generic.List<string>();

			// What Controls does lazily (Label's static constructor): ReplaceMapping discards the previous mapping. Then a user customisation.
			LabelHandler.Mapper.ReplaceMapping<Label, ILabelHandler>(nameof(ILabel.Text), (handler, label) => ran.Add("replaced"));
			LabelHandler.Mapper.AppendToMapping(nameof(ILabel.Text), (handler, label) => ran.Add("appended"));

			try
			{
				var widget = GtkTestApp.CreateWidget<global::Gtk.Label>(new Label { Text = "text survives" });

				Assert.Equal("text survives", widget.Text);
				Assert.Equal(new[] { "replaced", "appended" }, ran);
			}
			finally
			{
				// Static mappers are process-wide: put the stock mapping back for the other tests.
				LabelHandler.Mapper.ReplaceMapping<Label, ILabelHandler>(nameof(ILabel.Text), (handler, label) => { });
			}
		});

		[GtkFact]
		public void StockMapperPrependRunsAfterTheGtkMapping() => GtkTestHost.Run(() =>
		{
			string? seenByPrepend = null;
			LabelHandler.Mapper.PrependToMapping(nameof(ILabel.Text), (handler, label) => seenByPrepend = (handler.PlatformView as global::Gtk.Label)?.Text);

			try
			{
				GtkTestApp.CreateWidget<global::Gtk.Label>(new Label { Text = "gtk first" });

				// Documented ordering: the GTK implementation runs before the whole stock chain, prepends included.
				Assert.Equal("gtk first", seenByPrepend);
			}
			finally
			{
				LabelHandler.Mapper.ReplaceMapping<Label, ILabelHandler>(nameof(ILabel.Text), (handler, label) => { });
			}
		});

		[GtkFact]
		public void GtkMappersAreRestrictedToGtkPlatformViews() => GtkTestHost.Run(() =>
		{
			var seen = new System.Collections.Generic.List<string>();
			var builder = new GtkMapperBuilder<ILabel, ILabelHandler>(LabelHandler.Mapper)
				.MapGtk<global::Gtk.Label>(nameof(ILabel.Text), (handler, label, widget) => seen.Add("gtk"));
			var mapper = builder.Build();

			var label = new Label { Text = "x" };

			var other = new FakeLabelHandler(new object());
			other.SetVirtualView(label);
			mapper.GetProperty(nameof(ILabel.Text))!(other, label);
			Assert.Empty(seen);

			var gtk = new FakeLabelHandler(new global::Gtk.Label());
			gtk.SetVirtualView(label);
			mapper.GetProperty(nameof(ILabel.Text))!(gtk, label);
			Assert.Equal(new[] { "gtk" }, seen);
		});

		sealed class FakeLabelHandler : LabelHandler
		{
			readonly object _platformView;

			public FakeLabelHandler(object platformView) => _platformView = platformView;

			protected override object CreatePlatformView() => _platformView;
		}
	}
}
