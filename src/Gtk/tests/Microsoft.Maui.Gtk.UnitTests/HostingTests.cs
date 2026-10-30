using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Animations;
using Microsoft.Maui.Controls;
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
			Assert.False(GtkHandlerMap.TryGetGtkHandler(typeof(SliderHandler), out _));
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
