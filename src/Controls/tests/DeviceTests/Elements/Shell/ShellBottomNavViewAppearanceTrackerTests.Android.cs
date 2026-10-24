using System;
using System.Threading.Tasks;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Google.Android.Material.BottomNavigation;
using Google.Android.Material.Shape;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using Xunit;
using AColor = Android.Graphics.Color;
using Bitmap = Android.Graphics.Bitmap;
using Canvas = Android.Graphics.Canvas;
using ColorChangeRevealDrawable = Microsoft.Maui.Controls.Platform.ColorChangeRevealDrawable;
using GradientStrokeDrawable = Microsoft.Maui.Controls.Platform.GradientStrokeDrawable;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	public class ShellBottomNavViewAppearanceTrackerTests : ControlsHandlerTestBase
	{
		[Fact]
		public async Task GradientBackgroundIsReusedAndMutableChangesRefreshItsShader()
		{
			EnsureHandlerCreated();

			await InvokeOnMainThreadAsync(() =>
			{
				var shellItem = new ShellItem();
				using var bottomView = new BottomNavigationView(MauiContext.Context);
				var shellContext = new ShellRenderer(MauiContext.Context);
				using var tracker = new ShellBottomNavViewAppearanceTracker(shellContext, shellItem);
				var gradient = CreateGradient();
				gradient.EndPoint = new Point(1, 0);
				Shell.SetBackground(shellItem, gradient);
				var appearance = new ShellAppearance();
				appearance.Ingest(shellItem);
				tracker.SetAppearance(bottomView, appearance);
				var background = Assert.IsType<GradientStrokeDrawable>(bottomView.Background);
				background.SetBounds(0, 0, 100, 20);
				using var bitmap = Bitmap.CreateBitmap(100, 20, Bitmap.Config.Argb8888);
				using var canvas = new Canvas(bitmap);
				background.Draw(canvas);
				Assert.True(new AColor(bitmap.GetPixel(1, 10)).R > 200);

				Shell.SetForegroundColor(shellItem, Colors.Yellow);
				var updatedAppearance = new ShellAppearance();
				updatedAppearance.Ingest(shellItem);
				tracker.SetAppearance(bottomView, updatedAppearance);
				Assert.Same(background, bottomView.Background);

				gradient.GradientStops[0].Color = Colors.Green;
				tracker.SetAppearance(bottomView, updatedAppearance);
				Assert.Same(background, bottomView.Background);
				background.Draw(canvas);
				var pixel = new AColor(bitmap.GetPixel(1, 10));
				Assert.True(pixel.G > 100 && pixel.R < 30);
			});
		}

		[Fact]
		public async Task SolidBackgroundIsPreservedOnRepeatedColorUpdates()
		{
			EnsureHandlerCreated();

			await InvokeOnMainThreadAsync(() =>
			{
				var section = new ShellSection
				{
					Title = "Home",
					Items = { new ShellContent { Content = new ContentPage() } }
				};
				var shellItem = new ShellItem { Items = { section }, CurrentItem = section };
				using var bottomView = new BottomNavigationView(MauiContext.Context);
				using var title = new Java.Lang.String("Home");
				bottomView.Menu.Add(0, 0, 0, title);
				using var tracker = new TestAppearanceTracker(shellItem);

				bottomView.UpdateBackground(new LinearGradientBrush(
					new GradientStopCollection
					{
						new GradientStop(Colors.Red, 0),
						new GradientStop(Colors.Blue, 1)
					},
					new Point(0, 0),
					new Point(1, 1)));
				tracker.ApplyBackgroundColor(bottomView, Colors.Blue);
				var background = Assert.IsType<MaterialShapeDrawable>(bottomView.Background);

				tracker.ApplyBackgroundColor(bottomView, Colors.Blue);

				Assert.Same(background, bottomView.Background);
			});
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task GradientDisposesReplacedSolidBackground(bool useAnimation)
		{
			EnsureHandlerCreated();

			await InvokeOnMainThreadAsync(() =>
			{
				var shellItem = new ShellItem();
				using var bottomView = new BottomNavigationView(MauiContext.Context);
				var shellContext = new ShellRenderer(MauiContext.Context);
				using var tracker = new ShellBottomNavViewAppearanceTracker(shellContext, shellItem);
				using Drawable previousBackground = useAnimation
					? new ColorChangeRevealDrawable(Colors.Red.ToPlatform(), Colors.Blue.ToPlatform(), new Point(0, 0))
					: new MaterialShapeDrawable { FillColor = ColorStateList.ValueOf(Colors.Blue.ToPlatform()) };
				bottomView.Background = previousBackground;
				Shell.SetBackground(shellItem, CreateGradient());
				var appearance = new ShellAppearance();
				appearance.Ingest(shellItem);

				tracker.SetAppearance(bottomView, appearance);

				Assert.IsType<GradientStrokeDrawable>(bottomView.Background);
				Assert.Equal(IntPtr.Zero, previousBackground.Handle);
			});
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task GradientPreservesCapturedNativeBackground(bool useGradientBackground)
		{
			EnsureHandlerCreated();

			await InvokeOnMainThreadAsync(() =>
			{
				var shellItem = new ShellItem();
				using var bottomView = new BottomNavigationView(MauiContext.Context);
				var shellContext = new ShellRenderer(MauiContext.Context);
				using var tracker = new ShellBottomNavViewAppearanceTracker(shellContext, shellItem);
				if (useGradientBackground)
					Microsoft.Maui.Platform.ViewExtensions.UpdateBackground(bottomView, (Paint)CreateGradient());
				else
					bottomView.Background = new MaterialShapeDrawable { FillColor = ColorStateList.ValueOf(Colors.Blue.ToPlatform()) };
				using var originalBackground = bottomView.Background;
				tracker.CaptureNativeAppearance(bottomView);
				Shell.SetBackground(shellItem, CreateGradient());
				var appearance = new ShellAppearance();
				appearance.Ingest(shellItem);

				tracker.SetAppearance(bottomView, appearance);

				Assert.IsType<GradientStrokeDrawable>(bottomView.Background);
				Assert.NotSame(originalBackground, bottomView.Background);
				Assert.NotEqual(IntPtr.Zero, originalBackground.Handle);
			});
		}

		static LinearGradientBrush CreateGradient() =>
			new LinearGradientBrush(
				new GradientStopCollection
				{
					new GradientStop(Colors.Red, 0),
					new GradientStop(Colors.Blue, 1)
				},
				new Point(0, 0),
				new Point(1, 1));

		sealed class TestAppearanceTracker : ShellBottomNavViewAppearanceTracker
		{
			public TestAppearanceTracker(ShellItem shellItem) : base(null, shellItem)
			{
			}

			public void ApplyBackgroundColor(BottomNavigationView bottomView, Color color) =>
				SetBackgroundColor(bottomView, color);
		}
	}
}
