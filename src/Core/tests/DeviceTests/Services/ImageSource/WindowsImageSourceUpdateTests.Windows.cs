using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public class WindowsImageSourceUpdateTests : CoreHandlerTestBase
	{
		static FontImageSourceStub FontSource() => new()
		{
			Font = Font.OfSize("Dokdo", 24),
			Glyph = "X",
			Color = Colors.Red
		};

		[Fact]
		public async Task ClearedFlyoutIconDoesNotApplyPendingFont()
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<SliderHandler>(new SliderStub());
				var provider = handler.GetRequiredService<IImageSourceServiceProvider>();
				var icon = new AnimatedIcon();
				var pending = icon.UpdateFlyoutIconAsync(FontSource(), provider);
				Assert.False(pending.IsCompleted);
				await icon.UpdateFlyoutIconAsync(null, provider);
				var source = icon.Source;
				var fallback = Assert.IsType<FontIconSource>(icon.FallbackIconSource);
				await pending;
				Assert.Same(source, icon.Source);
				Assert.Same(fallback, icon.FallbackIconSource);
				Assert.Equal(16, icon.Width);
				Assert.Equal(16, icon.Height);
			});
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task SupersededFlyoutIconResultIsDisposed(bool clear)
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var completion = new TaskCompletionSource<IImageSourceServiceResult<ImageSource>>();
				var provider = new ControlledImageSourceService { Result = completion.Task };
				var icon = new AnimatedIcon();
				var pending = icon.UpdateFlyoutIconAsync(FontSource(), provider);
				var currentResult = new ImageSourceServiceResult(new Microsoft.UI.Xaml.Media.Imaging.WriteableBitmap(12, 18));
				provider.Result = Task.FromResult<IImageSourceServiceResult<ImageSource>>(currentResult);
				await icon.UpdateFlyoutIconAsync(clear ? null : FontSource(), provider);
				var current = icon.FallbackIconSource;
				var result = new ImageSourceServiceResult(new Microsoft.UI.Xaml.Media.Imaging.WriteableBitmap(37, 43));
				completion.SetResult(result);
				await pending;
				Assert.Same(current, icon.FallbackIconSource);
				Assert.True(result.IsDisposed);
				Assert.False(currentResult.IsDisposed);
			});
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task NavigationPaneBackgroundSupersedesPendingFont(bool clear)
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<SliderHandler>(new SliderStub());
				var provider = handler.GetRequiredService<IImageSourceServiceProvider>();
				var navigationView = new MauiNavigationView();
				await navigationView.AttachAndRun(async () =>
				{
					var pane = navigationView.PaneContentGrid;
					Assert.NotNull(pane);
					navigationView.UpdatePaneBackground(new SolidPaint(Colors.Blue));
					var expected = pane.Background;
					var pending = navigationView.UpdateBackgroundImageSourceAsync(FontSource(), provider, Aspect.Fill);
					Assert.False(pending.IsCompleted);
					if (clear)
					{
						await navigationView.UpdateBackgroundImageSourceAsync(null, provider, Aspect.Fill);
						Assert.Same(expected, pane.Background);
					}
					else
						navigationView.UpdatePaneBackground(new SolidPaint(Colors.Blue));
					var current = pane.Background;
					Assert.Equal(Colors.Blue.ToWindowsColor(), Assert.IsType<SolidColorBrush>(current).Color);
					await pending;
					Assert.Same(current, pane.Background);
				}, MauiContext);
			});
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task ClearingNavigationPaneImageRestoresBackground(bool configured)
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<SliderHandler>(new SliderStub());
				var provider = handler.GetRequiredService<IImageSourceServiceProvider>();
				var navigationView = new MauiNavigationView();
				await navigationView.AttachAndRun(async () =>
				{
					navigationView.UpdatePaneBackground(configured ? new SolidPaint(Colors.Blue) : null);
					var expected = navigationView.PaneContentGrid.Background;
					Assert.NotNull(expected);
					await navigationView.UpdateBackgroundImageSourceAsync(FontSource(), provider, Aspect.Fill);
					Assert.IsType<ImageBrush>(navigationView.PaneContentGrid.Background);
					await navigationView.UpdateBackgroundImageSourceAsync(null, provider, Aspect.Fill);
					Assert.Same(expected, navigationView.PaneContentGrid.Background);
				}, MauiContext);
			});
		}

		[Theory]
		[InlineData("Control", false)]
		[InlineData("Control", true)]
		[InlineData("Panel", false)]
		[InlineData("Panel", true)]
		[InlineData("Border", false)]
		[InlineData("Border", true)]
		public async Task SynchronousBackgroundSupersedesPendingFont(string target, bool clear)
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<SliderHandler>(new SliderStub());
				var provider = handler.GetRequiredService<IImageSourceServiceProvider>();
				FrameworkElement view = target switch
				{
					"Control" => new Button(),
					"Panel" => new Grid(),
					_ => new Border()
				};
				var pending = view.UpdateBackgroundImageAsync(FontSource(), provider);
				Assert.False(pending.IsCompleted);
				view.UpdateBackground(new SliderStub { Background = clear ? null : new SolidPaint(Colors.Blue) });
				var current = Background(view);
				if (clear)
					Assert.Null(current);
				else
					Assert.IsType<SolidColorBrush>(current);
				await pending;
				Assert.Same(current, Background(view));
			});
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task SupersededBackgroundResultIsDisposed(bool clear)
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var completion = new TaskCompletionSource<IImageSourceServiceResult<ImageSource>>();
				var provider = new ControlledImageSourceService { Result = completion.Task };
				var view = new Grid();
				var pending = view.UpdateBackgroundImageAsync(FontSource(), provider);
				if (clear)
					await view.UpdateBackgroundImageAsync(null, null);
				else
					view.UpdateBackground(new SolidPaint(Colors.Blue));
				var result = new ImageSourceServiceResult(new Microsoft.UI.Xaml.Media.Imaging.WriteableBitmap(12, 18));
				completion.SetResult(result);
				await pending;
				Assert.True(result.IsDisposed);
				Assert.False(view.Background is ImageBrush);
			});
		}

		[Fact]
		public async Task DisconnectingHandlerInvalidatesPendingBackground()
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<ButtonHandler>(new ButtonStub());
				var view = handler.PlatformView;
				var provider = handler.GetRequiredService<IImageSourceServiceProvider>();
				var pending = view.UpdateBackgroundImageSourceAsync(FontSource(), provider);
				Assert.False(pending.IsCompleted);
				((IElementHandler)handler).DisconnectHandler();
				var current = view.Background;
				await pending;
				Assert.Same(current, view.Background);
			});
		}

		[Theory]
		[InlineData("Button", false)]
		[InlineData("Button", true)]
		[InlineData("Picker", false)]
		[InlineData("Picker", true)]
		[InlineData("TextBox", false)]
		[InlineData("TextBox", true)]
		[InlineData("Slider", false)]
		[InlineData("Slider", true)]
		[InlineData("DatePicker", false)]
		[InlineData("DatePicker", true)]
		[InlineData("TimePicker", false)]
		[InlineData("TimePicker", true)]
		[InlineData("RadioButton", false)]
		[InlineData("RadioButton", true)]
		[InlineData("SearchBar", false)]
		[InlineData("SearchBar", true)]
		[InlineData("Stepper", false)]
		[InlineData("Stepper", true)]
		public async Task ResourceBackgroundUpdateSupersedesPendingFont(string target, bool clear)
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<SliderHandler>(new SliderStub());
				var provider = handler.GetRequiredService<IImageSourceServiceProvider>();
				var paint = clear ? null : new SolidPaint(Colors.Blue);
				(Control view, Action update) = target switch
				{
					"Button" => Target(new Button(), v => v.UpdateBackground(new ButtonStub { Background = paint })),
					"Picker" => Target(new ComboBox(), v => v.UpdateBackground(new PickerStub { Background = paint })),
					"TextBox" => Target(new TextBox(), v => v.UpdateBackground(new SliderStub { Background = paint })),
					"Slider" => Target(new MauiSlider(), v => v.UpdateBackgroundColor(new SliderStub { Background = paint })),
					"DatePicker" => Target(new CalendarDatePicker(), v => v.UpdateBackground(new DatePickerStub { Background = paint })),
					"TimePicker" => Target(new TimePicker(), v => v.UpdateBackground(new TimePickerStub { Background = paint })),
					"RadioButton" => Target(new RadioButton(), v => v.UpdateBackground(new RadioButtonStub { Background = paint })),
					"SearchBar" => Target(new AutoSuggestBox(), v => v.UpdateBackground(new SearchBarStub { Background = paint })),
					_ => Target(new MauiStepper(), v => v.UpdateBackground(new StepperStub { Background = paint }))
				};
				var pending = view.UpdateBackgroundImageSourceAsync(FontSource(), provider);
				Assert.False(pending.IsCompleted);
				update();
				var current = view.Background;
				await pending;
				Assert.Same(current, view.Background);
			});
		}

		static (Control, Action) Target<T>(T view, Action<T> update) where T : Control =>
			(view, () => update(view));

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task BackgroundMapperInvalidatesPendingFont(bool clear)
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var button = new ButtonStub();
				var handler = CreateHandler<ButtonHandler>(button);
				var provider = handler.GetRequiredService<IImageSourceServiceProvider>();
				var pending = handler.PlatformView.UpdateBackgroundImageSourceAsync(FontSource(), provider);
				Assert.False(pending.IsCompleted);
				button.Background = clear ? null : new SolidPaint(Colors.Blue);
				handler.UpdateValue(nameof(IView.Background));
				var current = handler.PlatformView.Background;
				await pending;
				Assert.Same(current, handler.PlatformView.Background);
			});
		}

		[Fact]
		public async Task CurrentBackgroundImageIsApplied()
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<SliderHandler>(new SliderStub());
				var provider = handler.GetRequiredService<IImageSourceServiceProvider>();
				var view = new Grid();
				await view.UpdateBackgroundImageSourceAsync(FontSource(), provider);
				Assert.NotNull(Assert.IsType<ImageBrush>(view.Background).ImageSource);
			});
		}

		static Brush Background(FrameworkElement view) => view switch
		{
			Control control => control.Background,
			Panel panel => panel.Background,
			Border border => border.Background,
			_ => null
		};
	}
}
