#nullable enable
using System;
using System.Threading.Tasks;
using Android.Graphics.Drawables;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;
using AndroidPlatform = Microsoft.Maui.Controls.PlatformConfiguration.Android;
using ControlsContentButton = Microsoft.Maui.Controls.ContentButton;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Button)]
	public class ContentButtonAndroidTests : ControlsHandlerTestBase
	{
		void SetupBuilder() => EnsureHandlerCreated(builder => builder.ConfigureMauiHandlers(handlers =>
		{
			handlers.AddHandler<ControlsContentButton, ContentButtonHandler>();
			handlers.AddHandler<Grid, LayoutHandler>();
			handlers.AddHandler<Label, LabelHandler>();
		}));

		[Fact]
		public async Task ChildTextChangeAutomaticallyResizesNativeButton()
		{
			SetupBuilder();
			var child = new Label { Text = "A" };
			var button = new ControlsContentButton
			{
				Content = child,
				Padding = 8,
				BorderWidth = 2,
				HorizontalOptions = LayoutOptions.Start,
				VerticalOptions = LayoutOptions.Start,
			};
			var host = new Grid
			{
				HorizontalOptions = LayoutOptions.Start,
				VerticalOptions = LayoutOptions.Start,
				Children = { button },
			};

			await CreateHandlerAndAddToWindow<LayoutHandler>(host, async _ =>
			{
				var handler = Assert.IsType<ContentButtonHandler>(button.Handler);
				var platformView = handler.PlatformView;
				await platformView.WaitForLayoutOrNonZeroSize();

				Assert.NotEqual(LayoutConstraint.Fixed, child.Constraint);

				var initial = GetCurrentSize();
				var grown = await SetTextAndWaitForSize(
					"A much longer presentation label",
					size => size.Managed.Width > initial.Managed.Width);
				var shrunk = await SetTextAndWaitForSize(
					"A",
					size => size.Managed.Width < grown.Managed.Width);

				Assert.True(grown.Managed.Width > initial.Managed.Width);
				Assert.True(grown.Native.Width > initial.Native.Width);
				Assert.True(shrunk.Managed.Width < grown.Managed.Width);
				Assert.True(shrunk.Native.Width < grown.Native.Width);

				(Size Managed, Size Native) GetCurrentSize() =>
					(
						new Size(button.Width, button.Height),
						new Size(platformView.MeasuredWidth, platformView.MeasuredHeight)
					);

				async Task<(Size Managed, Size Native)> SetTextAndWaitForSize(
					string text,
					Func<(Size Managed, Size Native), bool> predicate)
				{
					var sizeChanged = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
					button.SizeChanged += OnSizeChanged;

					try
					{
						child.Text = text;
						if (!predicate(GetCurrentSize()))
							await sizeChanged.Task.WaitAsync(TimeSpan.FromSeconds(2));

						return GetCurrentSize();
					}
					finally
					{
						button.SizeChanged -= OnSizeChanged;
					}

					void OnSizeChanged(object? sender, EventArgs args)
					{
						if (predicate(GetCurrentSize()))
							sizeChanged.TrySetResult(true);
					}
				}
			});
		}

		[Fact]
		public async Task RippleColorFluentApiPreservesAlphaTransparencyAndNativeReset()
		{
			SetupBuilder();
			var button = new ControlsContentButton
			{
				Content = new Grid { BackgroundColor = Colors.Black },
			};
			var handler = await CreateHandlerAsync<ContentButtonHandler>(button);

			await InvokeOnMainThreadAsync(() =>
			{
				var platformView = Assert.IsType<MauiContentButton>(handler.PlatformView);
				var nativeDefault = platformView.RippleColor!.DefaultColor;
				var translucent = Colors.Red.WithAlpha(0.35f);

				button.On<AndroidPlatform>().SetRippleColor(translucent);
				Assert.Equal(translucent.ToPlatform().ToArgb(), platformView.RippleColor!.DefaultColor);

				button.On<AndroidPlatform>().SetRippleColor(Colors.Transparent);
				Assert.Equal(Colors.Transparent.ToPlatform().ToArgb(), platformView.RippleColor!.DefaultColor);

				button.On<AndroidPlatform>().SetRippleColor(null);
				Assert.Equal(nativeDefault, platformView.RippleColor!.DefaultColor);
			});
		}

		[Fact]
		public async Task RippleColorUpdatesForegroundAndMauiBackgroundAcrossReplacement()
		{
			SetupBuilder();
			var expected = Colors.MediumPurple.WithAlpha(0.6f);
			var button = new ControlsContentButton
			{
				Content = new Grid { BackgroundColor = Colors.Black },
				Background = CreateGradient(Colors.Red, Colors.Orange),
				CornerRadius = 16,
			};
			button.On<AndroidPlatform>().SetRippleColor(expected);

			var handler = await CreateHandlerAsync<ContentButtonHandler>(button);

			await InvokeOnMainThreadAsync(() =>
			{
				var platformView = Assert.IsType<MauiContentButton>(handler.PlatformView);
				Assert.Equal(expected.ToPlatform().ToArgb(), platformView.RippleColor!.DefaultColor);
				Assert.IsType<RippleDrawable>(platformView.Background);
				Assert.Equal(1, platformView.ChildCount);

				button.Background = CreateGradient(Colors.Blue, Colors.Green);
				Assert.Equal(expected.ToPlatform().ToArgb(), platformView.RippleColor!.DefaultColor);
				Assert.IsType<RippleDrawable>(platformView.Background);

				button.Content = new Grid { BackgroundColor = Colors.White };
				Assert.Equal(1, platformView.ChildCount);
				Assert.Equal(expected.ToPlatform().ToArgb(), platformView.RippleColor!.DefaultColor);
			});
		}

		static LinearGradientBrush CreateGradient(Color start, Color end) =>
			new()
			{
				StartPoint = new Point(0, 0),
				EndPoint = new Point(1, 1),
				GradientStops =
				[
					new GradientStop { Color = start, Offset = 0 },
					new GradientStop { Color = end, Offset = 1 },
				],
			};
	}
}
