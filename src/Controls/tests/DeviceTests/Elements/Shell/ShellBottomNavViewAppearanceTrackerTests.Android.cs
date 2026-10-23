using System.Threading.Tasks;
using Google.Android.Material.BottomNavigation;
using Google.Android.Material.Shape;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	public class ShellBottomNavViewAppearanceTrackerTests : ControlsHandlerTestBase
	{
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
