#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AndroidX.Core.View;
using Google.Android.Material.AppBar;
using Google.Android.Material.Shape;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;
using AToolbar = AndroidX.AppCompat.Widget.Toolbar;
using NativeShellHandler = Microsoft.Maui.Controls.Handlers.ShellHandler;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	[Trait(RendererHandlerVariant.TraitName, RendererHandlerVariant.AndroidShellHandler)]
	public class ShellAppearanceObserverTests : ControlsHandlerTestBase
	{
		[Fact]
		public async Task ClearAndReplaceRemovesOldObserversAndReplaysAppearance()
		{
			Console.WriteLine($"Shell observer test host: material3={RuntimeFeature.IsMaterial3Enabled}, chrome={RuntimeFeature.UseMauiAndroidSystemBarBackgrounds}");
			EnsureHandlerCreated(builder => builder.ConfigureMauiHandlers(handlers =>
			{
				SetupShellHandlers(handlers);
				handlers.AddHandler(typeof(Shell), typeof(AppearanceTrackingShellHandler));
				handlers.AddHandler(typeof(ShellItem), typeof(ShellItemHandler));
				handlers.AddHandler(typeof(ShellSection), typeof(ShellSectionHandler));
			}));

			var page = new ContentPage { Title = "Observer lifecycle" };
			var shell = await InvokeOnMainThreadAsync(() =>
				new Shell { CurrentItem = new ShellContent { Content = page } });

			await CreateHandlerAndAddToWindow(shell, async () =>
			{
				var handler = Assert.IsType<AppearanceTrackingShellHandler>(shell.Handler);
				var tracker = Assert.IsType<AppearanceTrackingToolbarTracker>(handler.AppearanceTracker);
				var expectedCount = GetObserverSnapshot(shell).Length;
				var retiredItems = new List<ShellItem>();

				for (var cycle = 0; cycle < 3; cycle++)
				{
					retiredItems.Add(shell.CurrentItem);
					shell.Items.Clear();
					shell.CurrentItem = new ShellContent { Content = page };
					var item = shell.CurrentItem;
					var section = item.CurrentItem;
					var immediateObservers = GetObserverSnapshot(shell);

					AssertNoRetiredPivots(immediateObservers, retiredItems);
					Assert.Single(immediateObservers, entry =>
						ReferenceEquals(entry.Observer, item.Handler) && ReferenceEquals(entry.Pivot, item));

					var callbacks = tracker.AppearanceCount;
					Shell.SetBackgroundColor(shell, Colors.Blue);
					Assert.True(tracker.AppearanceCount > callbacks, "The immediate appearance update did not reach the tracker.");
					AssertAppearance(tracker, Colors.Blue);

					await AssertEventually(() => section.Handler is ShellSectionHandler &&
						GetObserverSnapshot(shell).Any(entry =>
							ReferenceEquals(entry.Observer, section.Handler) && ReferenceEquals(entry.Pivot, section)));
					await OnLoadedAsync(page);

					var settledObservers = GetObserverSnapshot(shell);
					AssertNoRetiredPivots(settledObservers, retiredItems);
					Assert.Equal(expectedCount, settledObservers.Length);
					Assert.Single(settledObservers, entry =>
						ReferenceEquals(entry.Observer, item.Handler) && ReferenceEquals(entry.Pivot, item));
					Assert.Single(settledObservers, entry =>
						ReferenceEquals(entry.Observer, section.Handler) && ReferenceEquals(entry.Pivot, section));

					callbacks = tracker.AppearanceCount;
					Shell.SetBackgroundColor(shell, Colors.Orange);
					Assert.True(tracker.AppearanceCount > callbacks, "The settled appearance update did not reach the tracker.");
					AssertAppearance(tracker, Colors.Orange);
				}
			});
		}

		static (IAppearanceObserver Observer, Element Pivot)[] GetObserverSnapshot(Shell shell)
		{
			var field = typeof(Shell).GetField("_appearanceObservers", BindingFlags.Instance | BindingFlags.NonPublic)
				?? throw new InvalidOperationException("Shell appearance observer field was not found.");
			var value = field.GetValue(shell);
			if (value is IEnumerable<(IAppearanceObserver Observer, Element Pivot)> observers)
				return observers.ToArray();

			if (value is not System.Collections.IEnumerable registrations)
				throw new InvalidOperationException("Shell appearance observer storage has changed.");

			var registrationType = typeof(Shell).GetNestedType("AppearanceObserverRegistration", BindingFlags.NonPublic)
				?? throw new InvalidOperationException("Shell appearance observer registration type was not found.");
			var observerField = registrationType.GetField("Observer", BindingFlags.Instance | BindingFlags.Public)
				?? throw new InvalidOperationException("Shell appearance observer registration has no Observer field.");
			var pivotField = registrationType.GetField("Pivot", BindingFlags.Instance | BindingFlags.Public)
				?? throw new InvalidOperationException("Shell appearance observer registration has no Pivot field.");
			var snapshot = new List<(IAppearanceObserver Observer, Element Pivot)>();
			foreach (var registration in registrations)
			{
				if (observerField.GetValue(registration) is not IAppearanceObserver observer ||
					pivotField.GetValue(registration) is not Element pivot)
					throw new InvalidOperationException("Shell appearance observer registration has invalid values.");
				snapshot.Add((observer, pivot));
			}
			return snapshot.ToArray();
		}

		static void AssertNoRetiredPivots(
			(IAppearanceObserver Observer, Element Pivot)[] observers, List<ShellItem> retiredItems)
		{
			foreach (var retired in retiredItems)
			{
				Assert.DoesNotContain(observers, entry =>
					ReferenceEquals(entry.Pivot, retired) ||
					ReferenceEquals(entry.Pivot.FindParentOfType<ShellItem>(), retired));
			}
		}

		static void AssertAppearance(AppearanceTrackingToolbarTracker tracker, Color color)
		{
			Assert.Equal(color, tracker.LastBackground);
			Assert.Equal(color, tracker.LastToolbarBackground);
			if (RuntimeFeature.UseMauiAndroidSystemBarBackgrounds)
				Assert.Equal(color.ToPlatform().ToArgb(), tracker.LastNativeBackground);
			else
				Assert.Null(tracker.LastNativeBackground);
		}
	}

	public class AppearanceTrackingShellHandler : NativeShellHandler
	{
		public AppearanceTrackingToolbarTracker? AppearanceTracker { get; private set; }

		protected override IShellToolbarAppearanceTracker CreateToolbarAppearanceTracker() =>
			AppearanceTracker = new AppearanceTrackingToolbarTracker(this);
	}

	public class AppearanceTrackingToolbarTracker : ShellToolbarAppearanceTracker
	{
		readonly IShellContext _context;

		public AppearanceTrackingToolbarTracker(IShellContext context) : base(context) => _context = context;

		public int AppearanceCount { get; private set; }
		public Color? LastBackground { get; private set; }
		public Color? LastToolbarBackground { get; private set; }
		public int? LastNativeBackground { get; private set; }

		public override void SetAppearance(AToolbar toolbar, IShellToolbarTracker toolbarTracker, ShellAppearance appearance)
		{
			AppearanceCount++;
			// Brush.Default is an empty SolidColorBrush; match the production fallback.
			LastBackground = !Brush.IsNullOrEmpty(appearance.Background) && appearance.Background is SolidColorBrush brush
				? brush.Color : appearance.BackgroundColor;

			base.SetAppearance(toolbar, toolbarTracker, appearance);
			LastToolbarBackground = _context.Shell.Toolbar.BarBackground is SolidColorBrush toolbarBrush
				? toolbarBrush.Color : null;
			LastNativeBackground = null;
			if (RuntimeFeature.UseMauiAndroidSystemBarBackgrounds)
			{
				var appBar = toolbar.Parent.GetParentOfType<AppBarLayout>()
					?? throw new InvalidOperationException("The toolbar has no AppBarLayout parent.");
				if (RuntimeFeature.IsMaterial3Enabled)
				{
					var background = Assert.IsAssignableFrom<MaterialShapeDrawable>(appBar.Background);
					var fill = background.FillColor
						?? throw new InvalidOperationException("The Material AppBar has no fill color.");
					LastNativeBackground = fill.GetColorForState(
						appBar.GetDrawableState(), new global::Android.Graphics.Color(fill.DefaultColor));
				}
				else
				{
					var tint = ViewCompat.GetBackgroundTintList(appBar)
						?? throw new InvalidOperationException("The AppBar has no background tint.");
					LastNativeBackground = tint.GetColorForState(
						appBar.GetDrawableState(), new global::Android.Graphics.Color(tint.DefaultColor));
				}
			}
			Console.WriteLine($"Shell appearance: material3={RuntimeFeature.IsMaterial3Enabled}, chrome={RuntimeFeature.UseMauiAndroidSystemBarBackgrounds}, background={LastBackground}, toolbar={LastToolbarBackground}, nativeArgb={LastNativeBackground}");
		}
	}
}
