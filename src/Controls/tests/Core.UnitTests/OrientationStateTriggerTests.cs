using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests;

public class OrientationStateTriggerTests : BaseTestFixture
{
	[Fact]
	public void ConstructionCanHappen()
	{
		var trigger = new OrientationStateTrigger();

		Assert.NotNull(trigger);
	}

	[Theory]
	[InlineData(DisplayOrientation.Portrait, DisplayOrientation.Portrait, true)]
	[InlineData(DisplayOrientation.Portrait, DisplayOrientation.Landscape, false)]
	[InlineData(DisplayOrientation.Portrait, DisplayOrientation.Unknown, false)]
	[InlineData(DisplayOrientation.Landscape, DisplayOrientation.Landscape, true)]
	[InlineData(DisplayOrientation.Landscape, DisplayOrientation.Portrait, false)]
	[InlineData(DisplayOrientation.Landscape, DisplayOrientation.Unknown, false)]
	[InlineData(DisplayOrientation.Unknown, DisplayOrientation.Unknown, false)]
	[InlineData(DisplayOrientation.Unknown, DisplayOrientation.Portrait, false)]
	[InlineData(DisplayOrientation.Unknown, DisplayOrientation.Landscape, false)]
	public void CorrectStateIsAppliedWhenAttached(DisplayOrientation triggerOrientation, DisplayOrientation currentOrientation, bool isApplied)
	{
		DeviceDisplay.SetCurrent(new MockDeviceDisplay(
			new(100, 200, 2, currentOrientation, DisplayRotation.Rotation0)));

		var redBrush = new SolidColorBrush(Colors.Red);
		var greenBrush = new SolidColorBrush(Colors.Green);

		var label = new Label { Background = redBrush };

		VisualStateManager.SetVisualStateGroups(label, new VisualStateGroupList
		{
			new VisualStateGroup
			{
				States =
				{
					new VisualState
					{
						Name = "OrientationState",
						StateTriggers = { new OrientationStateTrigger { Orientation = triggerOrientation } },
						Setters = { new Setter { Property = Label.BackgroundProperty, Value = greenBrush } }
					}
				}
			}
		});

		Assert.Equal(label.Background, isApplied ? greenBrush : redBrush);
	}

	[Fact]
	public void OrientationPropertyChangeTriggersStateUpdate()
	{
		DeviceDisplay.SetCurrent(new MockDeviceDisplay(
			new(100, 200, 2, DisplayOrientation.Landscape, DisplayRotation.Rotation0)));

		var redBrush = new SolidColorBrush(Colors.Red);
		var greenBrush = new SolidColorBrush(Colors.Green);

		var label = new Label { Background = redBrush };

		var trigger = new OrientationStateTrigger { Orientation = DisplayOrientation.Portrait };

		VisualStateManager.SetVisualStateGroups(label, new VisualStateGroupList
		{
			new VisualStateGroup
			{
				States =
				{
					new VisualState
					{
						Name = "OrientationState",
						StateTriggers = { trigger },
						Setters = { new Setter { Property = Label.BackgroundProperty, Value = greenBrush } }
					}
				}
			}
		});

		// Initially should have red background (current Landscape != trigger Portrait)
		Assert.Equal(redBrush, label.Background);

		// Change trigger's orientation to match current device orientation
		trigger.Orientation = DisplayOrientation.Landscape;

		// Now should have green background
		Assert.Equal(greenBrush, label.Background);
	}

	[Fact]
	public void TriggerDeactivatesWhenDetached()
	{
		DeviceDisplay.SetCurrent(new MockDeviceDisplay(
			new(100, 200, 2, DisplayOrientation.Portrait, DisplayRotation.Rotation0)));

		var greenBrush = new SolidColorBrush(Colors.Green);
		var label = new Label();
		var trigger = new OrientationStateTrigger { Orientation = DisplayOrientation.Portrait };

		VisualStateManager.SetVisualStateGroups(label, new VisualStateGroupList
		{
			new VisualStateGroup
			{
				States =
				{
					new VisualState
					{
						Name = "OrientationState",
						StateTriggers = { trigger },
						Setters = { new Setter { Property = Label.BackgroundProperty, Value = greenBrush } }
					}
				}
			}
		});

		Assert.False(trigger.IsAttached);

		var window = new Window(new ContentPage() { Content = label });

		Assert.True(trigger.IsAttached);

		window.Page = new ContentPage();

		Assert.False(trigger.IsAttached);
	}

	[Fact]
	public void MultipleTriggersWithDifferentOrientations()
	{
		DeviceDisplay.SetCurrent(new MockDeviceDisplay(
			new(100, 200, 2, DisplayOrientation.Landscape, DisplayRotation.Rotation0)));

		var redBrush = new SolidColorBrush(Colors.Red);
		var greenBrush = new SolidColorBrush(Colors.Green);
		var blueBrush = new SolidColorBrush(Colors.Blue);

		var label = new Label { Background = redBrush };

		VisualStateManager.SetVisualStateGroups(label, new VisualStateGroupList
		{
			new VisualStateGroup
			{
				States =
				{
					new VisualState
					{
						Name = "PortraitState",
						StateTriggers = { new OrientationStateTrigger { Orientation = DisplayOrientation.Portrait } },
						Setters = { new Setter { Property = Label.BackgroundProperty, Value = greenBrush } }
					},
					new VisualState
					{
						Name = "LandscapeState",
						StateTriggers = { new OrientationStateTrigger { Orientation = DisplayOrientation.Landscape } },
						Setters = { new Setter { Property = Label.BackgroundProperty, Value = blueBrush } }
					}
				}
			}
		});

		// Should activate LandscapeState (blue background)
		Assert.Equal(blueBrush, label.Background);
	}

	[Fact]
	public void DeviceOrientationChangeTriggersStateUpdate()
	{
		var mockDeviceDisplay = new MockDeviceDisplay(
			new(100, 200, 2, DisplayOrientation.Portrait, DisplayRotation.Rotation0));
		DeviceDisplay.SetCurrent(mockDeviceDisplay);

		var redBrush = new SolidColorBrush(Colors.Red);
		var greenBrush = new SolidColorBrush(Colors.Green);

		var label = new Label { Background = redBrush };

		VisualStateManager.SetVisualStateGroups(label, new VisualStateGroupList
		{
			new VisualStateGroup
			{
				States =
				{
					new VisualState
					{
						Name = "LandscapeState",
						StateTriggers = { new OrientationStateTrigger { Orientation = DisplayOrientation.Landscape } },
						Setters = { new Setter { Property = Label.BackgroundProperty, Value = greenBrush } }
					}
				}
			}
		});

		var window = new Window(new ContentPage() { Content = label });

		// Initially should have red background (current Portrait != trigger Landscape)
		Assert.Equal(redBrush, label.Background);

		// Simulate device orientation change to landscape
		mockDeviceDisplay.SetMainDisplayOrientation(DisplayOrientation.Landscape);

		// Now should have green background
		Assert.Equal(greenBrush, label.Background);

		window.Page = new ContentPage();
	}

	[Fact, Category(TestCategory.Memory)]
	public async Task AttachedTriggerDoesNotKeepPageAlive()
	{
		DeviceDisplay.SetCurrent(new MockDeviceDisplay());

		// The window keeps its page, as it does after an Android Activity is recreated.
		var pageReference = CreatePageInWindow();

		Assert.False(await pageReference.WaitForCollect(), "Page should not be alive!");
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	static WeakReference CreatePageInWindow()
	{
		var layout = new VerticalStackLayout();
		var page = new ContentPage { Content = layout };

		// Like a XAML DataTemplate that needs the root object of its file.
		var template = new DataTemplate(() => new Label { BindingContext = page });

		var trigger = new OrientationStateTrigger { Orientation = DisplayOrientation.Landscape };
		VisualStateManager.SetVisualStateGroups(layout, new VisualStateGroupList
		{
			new VisualStateGroup
			{
				States =
				{
					new VisualState
					{
						Name = "TriggeredState",
						StateTriggers = { trigger },
						Setters = { new Setter { Property = BindableLayout.ItemTemplateProperty, Value = template } }
					}
				}
			}
		});

		_ = new Window(page);
		Assert.True(trigger.IsAttached);

		return new WeakReference(page);
	}

	[Fact]
	public async Task TriggerStillUpdatesAfterGarbageCollection()
	{
		var mockDeviceDisplay = new MockDeviceDisplay(
			new(100, 200, 2, DisplayOrientation.Portrait, DisplayRotation.Rotation0));
		DeviceDisplay.SetCurrent(mockDeviceDisplay);

		var redBrush = new SolidColorBrush(Colors.Red);
		var greenBrush = new SolidColorBrush(Colors.Green);

		var label = new Label { Background = redBrush };

		VisualStateManager.SetVisualStateGroups(label, new VisualStateGroupList
		{
			new VisualStateGroup
			{
				States =
				{
					new VisualState
					{
						Name = "TriggeredState",
						StateTriggers = { new OrientationStateTrigger { Orientation = DisplayOrientation.Landscape } },
						Setters = { new Setter { Property = Label.BackgroundProperty, Value = greenBrush } }
					}
				}
			}
		});

		var window = new Window(new ContentPage() { Content = label });

		await TestHelpers.Collect();

		mockDeviceDisplay.SetMainDisplayOrientation(DisplayOrientation.Landscape);

		Assert.Equal(greenBrush, label.Background);
		GC.KeepAlive(window);
	}
}
