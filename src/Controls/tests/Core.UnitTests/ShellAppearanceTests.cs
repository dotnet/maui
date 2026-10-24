using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class ShellAppearanceTests : ShellTestBase
	{
		[Fact]
		public void ColorSetCorrectly()
		{
			var testShell = new TestShell(CreateShellItem<FlyoutItem>());
			testShell.Items[0].SetValue(Shell.DisabledColorProperty, Colors.Purple);

			ShellAppearance result = new ShellAppearance();
			result.Ingest(testShell.Items[0]);
			Assert.Equal(Colors.Purple, result.DisabledColor);
		}

		ShellAppearance IngestAppearance(Action<BindableObject> setValues)
		{
			var testShell = new TestShell(CreateShellItem<FlyoutItem>());
			setValues(testShell.Items[0]);
			var result = new ShellAppearance();
			result.Ingest(testShell.Items[0]);
			return result;
		}

		static LinearGradientBrush CreateGradient() =>
			new LinearGradientBrush
			{
				GradientStops = new GradientStopCollection
				{
					new GradientStop(Colors.Yellow, 0f),
					new GradientStop(Colors.Green, 1f)
				}
			};

		[Fact]
		public void EffectiveTabBarBackground_UsesTabBarBackgroundColor()
		{
			var result = IngestAppearance(item =>
				item.SetValue(Shell.TabBarBackgroundColorProperty, Colors.Red));

			var brush = Assert.IsType<SolidColorBrush>(result.EffectiveTabBarBackground);
			Assert.Equal(Colors.Red, brush.Color);
		}

		[Fact]
		public void EffectiveTabBarBackground_UsesBackgroundBrush()
		{
			var gradient = CreateGradient();
			var result = IngestAppearance(item =>
				item.SetValue(Shell.BackgroundProperty, gradient));

			Assert.Same(gradient, result.EffectiveTabBarBackground);
		}

		[Fact]
		public void GradientStopChange_UpdatesTabBarAppearanceAndDetachesWhenObserverIsRemoved()
		{
			var shell = new Shell();
			var shellItem = CreateShellItem<FlyoutItem>();
			var gradient = CreateGradient();
			var observer = new TestAppearanceObserver();
			shell.Items.Add(shellItem);
			Shell.SetBackground(shell, gradient);

			var shellController = (IShellController)shell;
			shellController.AddAppearanceObserver(observer, shellItem);

			Assert.Same(gradient, observer.LastAppearance.EffectiveTabBarBackground);
			Assert.Equal(1, observer.UpdateCount);

			gradient.GradientStops[0].Color = Colors.Red;

			Assert.Same(gradient, observer.LastAppearance.EffectiveTabBarBackground);
			Assert.Equal(Colors.Red, ((GradientBrush)observer.LastAppearance.EffectiveTabBarBackground).GradientStops[0].Color);
			Assert.Equal(2, observer.UpdateCount);

			var replacementGradient = CreateGradient();
			Shell.SetBackground(shell, replacementGradient);

			Assert.Same(replacementGradient, observer.LastAppearance.EffectiveTabBarBackground);
			Assert.Equal(3, observer.UpdateCount);

			gradient.GradientStops[0].Color = Colors.Blue;
			Assert.Equal(3, observer.UpdateCount);

			replacementGradient.GradientStops[0].Color = Colors.Blue;
			Assert.Equal(4, observer.UpdateCount);

			Assert.True(shellController.RemoveAppearanceObserver(observer));
			replacementGradient.GradientStops[0].Color = Colors.Red;

			Assert.Equal(4, observer.UpdateCount);
		}

		[Fact]
		public void SolidColorBrushChangesUpdateAppearanceAndDetachWhenReplacedOrRemoved()
		{
			var shell = new Shell();
			var shellItem = CreateShellItem<FlyoutItem>();
			var brush = new SolidColorBrush(Colors.Red);
			var observer = new TestAppearanceObserver();
			shell.Items.Add(shellItem);
			Shell.SetBackground(shell, brush);
			var shellController = (IShellController)shell;
			shellController.AddAppearanceObserver(observer, shellItem);

			brush.Color = Colors.Blue;

			Assert.Equal(2, observer.UpdateCount);
			Assert.Equal(Colors.Blue, Assert.IsType<SolidColorBrush>(observer.LastAppearance.EffectiveTabBarBackground).Color);

			var replacement = new SolidColorBrush(Colors.Green);
			Shell.SetBackground(shell, replacement);
			Assert.Equal(3, observer.UpdateCount);
			Assert.Same(replacement, observer.LastAppearance.EffectiveTabBarBackground);

			brush.Color = Colors.Yellow;
			Assert.Equal(3, observer.UpdateCount);

			replacement.Color = Colors.Purple;
			Assert.Equal(4, observer.UpdateCount);

			Assert.True(shellController.RemoveAppearanceObserver(observer));
			replacement.Color = Colors.Blue;
			Assert.Equal(4, observer.UpdateCount);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void EmptySolidColorBrushCanBeRepopulated(bool initiallyEmpty)
		{
			var shell = new Shell();
			var shellItem = CreateShellItem<FlyoutItem>();
			var brush = new SolidColorBrush(initiallyEmpty ? null : Colors.Red);
			var observer = new TestAppearanceObserver();
			shell.Items.Add(shellItem);
			Shell.SetBackground(shell, brush);
			((IShellController)shell).AddAppearanceObserver(observer, shellItem);

			if (!initiallyEmpty)
				brush.Color = null;

			Assert.Null(observer.LastAppearance.EffectiveTabBarBackground);
			var updateCount = observer.UpdateCount;

			brush.Color = Colors.Blue;

			Assert.Equal(updateCount + 1, observer.UpdateCount);
			Assert.Same(brush, observer.LastAppearance.EffectiveTabBarBackground);
		}

		[Fact]
		public void TabBarBackgroundColorOverridesSolidBrushInvalidation()
		{
			var shell = new Shell();
			var shellItem = CreateShellItem<FlyoutItem>();
			var brush = new SolidColorBrush(Colors.Red);
			var observer = new TestAppearanceObserver();
			shell.Items.Add(shellItem);
			Shell.SetBackground(shell, brush);
			((IShellController)shell).AddAppearanceObserver(observer, shellItem);
			Shell.SetTabBarBackgroundColor(shell, Colors.Green);
			var updateCount = observer.UpdateCount;

			brush.Color = Colors.Blue;

			Assert.Equal(updateCount, observer.UpdateCount);
			Assert.Equal(Colors.Green, Assert.IsType<SolidColorBrush>(observer.LastAppearance.EffectiveTabBarBackground).Color);

			shell.ClearValue(Shell.TabBarBackgroundColorProperty);
			Assert.Same(brush, observer.LastAppearance.EffectiveTabBarBackground);
			updateCount = observer.UpdateCount;
			brush.Color = Colors.Purple;

			Assert.Equal(updateCount + 1, observer.UpdateCount);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void GradientStopsCanBeRepopulated(bool initiallyEmpty)
		{
			var shell = new Shell();
			var shellItem = CreateShellItem<FlyoutItem>();
			var gradient = CreateGradient();
			var observer = new TestAppearanceObserver();
			shell.Items.Add(shellItem);
			Shell.SetBackground(shell, gradient);

			if (initiallyEmpty)
				gradient.GradientStops.Clear();

			((IShellController)shell).AddAppearanceObserver(observer, shellItem);

			if (!initiallyEmpty)
				gradient.GradientStops.Clear();

			Assert.Null(observer.LastAppearance.EffectiveTabBarBackground);
			var updateCount = observer.UpdateCount;

			gradient.GradientStops.Add(new GradientStop(Colors.Red, 0));
			gradient.GradientStops.Add(new GradientStop(Colors.Blue, 1));

			Assert.Equal(updateCount + 2, observer.UpdateCount);
			Assert.Same(gradient, observer.LastAppearance.EffectiveTabBarBackground);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void EmptyChildGradientReplacesInheritedBackgroundWhenPopulated(bool initiallyEmpty)
		{
			var shell = new Shell();
			var shellItem = CreateShellItem<FlyoutItem>();
			var inheritedBrush = new SolidColorBrush(Colors.Purple);
			var gradient = CreateGradient();
			var observer = new TestAppearanceObserver();
			shell.Items.Add(shellItem);
			Shell.SetBackground(shell, inheritedBrush);
			if (initiallyEmpty)
				gradient.GradientStops.Clear();
			Shell.SetBackground(shellItem, gradient);
			var shellController = (IShellController)shell;
			shellController.AddAppearanceObserver(observer, shellItem);

			if (!initiallyEmpty)
				gradient.GradientStops.Clear();
			Assert.Same(inheritedBrush, observer.LastAppearance.EffectiveTabBarBackground);
			var updateCount = observer.UpdateCount;

			gradient.GradientStops.Add(new GradientStop(Colors.Blue, 0));

			Assert.Equal(updateCount + 1, observer.UpdateCount);
			Assert.Same(gradient, observer.LastAppearance.EffectiveTabBarBackground);
			updateCount = observer.UpdateCount;
			inheritedBrush.Color = Colors.Green;
			Assert.Equal(updateCount, observer.UpdateCount);

			gradient.GradientStops.Clear();
			Assert.Same(inheritedBrush, observer.LastAppearance.EffectiveTabBarBackground);
			shellItem.ClearValue(Shell.BackgroundProperty);
			updateCount = observer.UpdateCount;
			gradient.GradientStops.Add(new GradientStop(Colors.Red, 0));
			Assert.Equal(updateCount, observer.UpdateCount);

			inheritedBrush.Color = Colors.Yellow;
			Assert.Equal(updateCount + 1, observer.UpdateCount);
		}

		[Fact]
		public void NestedEmptyBackgroundsObserveEachCandidateByReference()
		{
			var shell = new Shell();
			var shellItem = CreateShellItem<FlyoutItem>();
			var inheritedBrush = new SolidColorBrush(Colors.Purple);
			var outerBrush = new LinearGradientBrush();
			var innerBrush = new LinearGradientBrush();
			var observer = new TestAppearanceObserver();
			shell.Items.Add(shellItem);
			Shell.SetBackground(shell, inheritedBrush);
			Shell.SetBackground(shellItem, outerBrush);
			Shell.SetBackground(shellItem.Items[0], innerBrush);
			((IShellController)shell).AddAppearanceObserver(observer, shellItem);
			Assert.Same(inheritedBrush, observer.LastAppearance.EffectiveTabBarBackground);
			var updateCount = observer.UpdateCount;

			outerBrush.GradientStops.Add(new GradientStop(Colors.Red, 0));
			Assert.Equal(updateCount + 1, observer.UpdateCount);
			Assert.Same(outerBrush, observer.LastAppearance.EffectiveTabBarBackground);

			innerBrush.GradientStops.Add(new GradientStop(Colors.Blue, 0));
			Assert.Equal(updateCount + 2, observer.UpdateCount);
			Assert.Same(innerBrush, observer.LastAppearance.EffectiveTabBarBackground);
		}

		[Fact]
		public void SharedEmptyBackgroundIsObservedOnlyOnce()
		{
			var shell = new Shell();
			var shellItem = CreateShellItem<FlyoutItem>();
			var sharedBrush = new LinearGradientBrush();
			var observer = new TestAppearanceObserver();
			shell.Items.Add(shellItem);
			Shell.SetBackground(shell, new SolidColorBrush(Colors.Purple));
			Shell.SetBackground(shellItem, sharedBrush);
			Shell.SetBackground(shellItem.Items[0], sharedBrush);
			((IShellController)shell).AddAppearanceObserver(observer, shellItem);
			var updateCount = observer.UpdateCount;

			sharedBrush.GradientStops.Add(new GradientStop(Colors.Blue, 0));

			Assert.Equal(updateCount + 1, observer.UpdateCount);
			Assert.Same(sharedBrush, observer.LastAppearance.EffectiveTabBarBackground);
		}

		[Fact]
		public void EmptyChildSolidBrushReplacesInheritedBackgroundWhenPopulated()
		{
			var shell = new Shell();
			var shellItem = CreateShellItem<FlyoutItem>();
			var inheritedBrush = new SolidColorBrush(Colors.Purple);
			var childBrush = new SolidColorBrush();
			var observer = new TestAppearanceObserver();
			shell.Items.Add(shellItem);
			Shell.SetBackground(shell, inheritedBrush);
			Shell.SetBackground(shellItem, childBrush);
			((IShellController)shell).AddAppearanceObserver(observer, shellItem);
			Assert.Same(inheritedBrush, observer.LastAppearance.EffectiveTabBarBackground);
			var updateCount = observer.UpdateCount;

			childBrush.Color = Colors.Blue;

			Assert.Equal(updateCount + 1, observer.UpdateCount);
			Assert.Same(childBrush, observer.LastAppearance.EffectiveTabBarBackground);
		}

		[Fact]
		public async Task SharedGradientDoesNotRetainShellOrAppearanceObservers()
		{
			var gradient = CreateGradient();
			var references = CreateShellAppearanceReferences(gradient);

			foreach (var reference in references)
				Assert.False(await reference.WaitForCollect());

			gradient.GradientStops[0].Color = Colors.Red;
			GC.KeepAlive(gradient);
		}

		[Fact]
		public async Task SharedSolidBrushDoesNotRetainShellOrAppearanceObservers()
		{
			var brush = new SolidColorBrush(Colors.Red);
			var references = CreateShellAppearanceReferences(brush);

			foreach (var reference in references)
				Assert.False(await reference.WaitForCollect());

			brush.Color = Colors.Blue;
			GC.KeepAlive(brush);
		}

		[Fact]
		public async Task SharedInheritedAndEmptyChildBrushesDoNotRetainShell()
		{
			var inheritedBrush = new SolidColorBrush(Colors.Purple);
			var childBrush = new LinearGradientBrush();
			var references = CreateShellAppearanceReferences(inheritedBrush, childBrush);

			foreach (var reference in references)
				Assert.False(await reference.WaitForCollect());

			childBrush.GradientStops.Add(new GradientStop(Colors.Blue, 0));
			inheritedBrush.Color = Colors.Green;
			GC.KeepAlive(childBrush);
			GC.KeepAlive(inheritedBrush);
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
		WeakReference[] CreateShellAppearanceReferences(Brush background, Brush childBackground = null)
		{
			var shell = new Shell();
			var shellItem = CreateShellItem<FlyoutItem>();
			var observer = new TestAppearanceObserver();
			shell.Items.Add(shellItem);
			Shell.SetBackground(shell, background);
			if (childBackground is not null)
				Shell.SetBackground(shellItem, childBackground);
			((IShellController)shell).AddAppearanceObserver(observer, shellItem);

			return new[] { new WeakReference(shell), new WeakReference(shellItem), new WeakReference(observer) };
		}

		[Fact]
		public async Task GradientSubscriptionSurvivesGarbageCollectionWhileShellIsAlive()
		{
			var shell = new Shell();
			var shellItem = CreateShellItem<FlyoutItem>();
			var gradient = CreateGradient();
			var observer = new TestAppearanceObserver();
			shell.Items.Add(shellItem);
			Shell.SetBackground(shell, gradient);
			((IShellController)shell).AddAppearanceObserver(observer, shellItem);

			await TestHelpers.Collect();
			gradient.GradientStops[0].Color = Colors.Red;

			Assert.Equal(2, observer.UpdateCount);
			GC.KeepAlive(shell);
		}

		[Fact]
		public void EffectiveTabBarBackground_UsesBackgroundColor()
		{
			var result = IngestAppearance(item =>
				item.SetValue(Shell.BackgroundColorProperty, Colors.Green));

			var brush = Assert.IsType<SolidColorBrush>(result.EffectiveTabBarBackground);
			Assert.Equal(Colors.Green, brush.Color);
		}

		[Fact]
		public void EffectiveTabBarBackground_TabBarBackgroundColorWinsOverBackgroundBrush()
		{
			var result = IngestAppearance(item =>
			{
				item.SetValue(Shell.TabBarBackgroundColorProperty, Colors.Red);
				item.SetValue(Shell.BackgroundProperty, CreateGradient());
			});

			var brush = Assert.IsType<SolidColorBrush>(result.EffectiveTabBarBackground);
			Assert.Equal(Colors.Red, brush.Color);
		}

		[Fact]
		public void EffectiveTabBarBackground_BackgroundBrushWinsOverBackgroundColor()
		{
			var gradient = CreateGradient();
			var result = IngestAppearance(item =>
			{
				item.SetValue(Shell.BackgroundProperty, gradient);
				item.SetValue(Shell.BackgroundColorProperty, Colors.Green);
			});

			Assert.Same(gradient, result.EffectiveTabBarBackground);
		}

		[Fact]
		public void EffectiveTabBarBackground_NullWhenNothingSet()
		{
			var result = IngestAppearance(_ => { });

			Assert.Null(result.EffectiveTabBarBackground);
		}

		[Fact]
		public void EffectiveTabBarBackgroundColor_FallsBackToBackgroundColor()
		{
			var result = IngestAppearance(item =>
				item.SetValue(Shell.BackgroundColorProperty, Colors.Green));

			Assert.Equal(Colors.Green, ((IShellAppearanceElement)result).EffectiveTabBarBackgroundColor);
		}

		[Fact]
		public void EffectiveTabBarBackgroundColor_TabBarBackgroundColorWinsOverBackgroundColor()
		{
			var result = IngestAppearance(item =>
			{
				item.SetValue(Shell.TabBarBackgroundColorProperty, Colors.Red);
				item.SetValue(Shell.BackgroundColorProperty, Colors.Green);
			});

			Assert.Equal(Colors.Red, ((IShellAppearanceElement)result).EffectiveTabBarBackgroundColor);
		}

		class TestAppearanceObserver : IAppearanceObserver
		{
			public ShellAppearance LastAppearance { get; private set; }
			public int UpdateCount { get; private set; }

			public void OnAppearanceChanged(ShellAppearance appearance)
			{
				LastAppearance = appearance;
				UpdateCount++;
			}
		}
	}
}
