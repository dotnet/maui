using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests;

public class DisplayRotationStateTriggerTests : BaseTestFixture
{
	[Fact]
	public void ConstructionCanHappen()
	{
		var trigger = new DisplayRotationStateTrigger();

		Assert.NotNull(trigger);
	}

	[Theory]
	[InlineData(DisplayRotation.Rotation0, DisplayRotation.Rotation0, true)]
	[InlineData(DisplayRotation.Rotation0, DisplayRotation.Rotation90, false)]
	[InlineData(DisplayRotation.Rotation0, DisplayRotation.Unknown, false)]
	[InlineData(DisplayRotation.Rotation90, DisplayRotation.Rotation90, true)]
	[InlineData(DisplayRotation.Rotation90, DisplayRotation.Rotation180, false)]
	[InlineData(DisplayRotation.Rotation180, DisplayRotation.Rotation180, true)]
	[InlineData(DisplayRotation.Rotation180, DisplayRotation.Rotation270, false)]
	[InlineData(DisplayRotation.Rotation270, DisplayRotation.Rotation270, true)]
	[InlineData(DisplayRotation.Rotation270, DisplayRotation.Rotation0, false)]
	[InlineData(DisplayRotation.Unknown, DisplayRotation.Unknown, true)]
	[InlineData(DisplayRotation.Unknown, DisplayRotation.Rotation0, false)]
	public void CorrectStateIsAppliedWhenAttached(DisplayRotation triggerRotation, DisplayRotation currentRotation, bool isApplied)
	{
		DeviceDisplay.SetCurrent(new MockDeviceDisplay(
			new(100, 200, 2, DisplayOrientation.Portrait, currentRotation)));

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
						Name = "RotationState",
						StateTriggers = { new DisplayRotationStateTrigger { Rotation = triggerRotation } },
						Setters = { new Setter { Property = Label.BackgroundProperty, Value = greenBrush } }
					}
				}
			}
		});

		Assert.Equal(label.Background, isApplied ? greenBrush : redBrush);
	}


	[Fact]
	public void RotationPropertyChangeTriggersStateUpdate()
	{
		DeviceDisplay.SetCurrent(new MockDeviceDisplay(
			new(100, 200, 2, DisplayOrientation.Portrait, DisplayRotation.Rotation90)));

		var redBrush = new SolidColorBrush(Colors.Red);
		var greenBrush = new SolidColorBrush(Colors.Green);

		var label = new Label { Background = redBrush };

		var trigger = new DisplayRotationStateTrigger { Rotation = DisplayRotation.Rotation0 };

		VisualStateManager.SetVisualStateGroups(label, new VisualStateGroupList
		{
			new VisualStateGroup
			{
				States =
				{
					new VisualState
					{
						Name = "RotationState",
						StateTriggers = { trigger },
						Setters = { new Setter { Property = Label.BackgroundProperty, Value = greenBrush } }
					}
				}
			}
		});

		// Initially should have red background (current Rotation90 != trigger Rotation0)
		Assert.Equal(redBrush, label.Background);

		// Change trigger's rotation to match current device rotation
		trigger.Rotation = DisplayRotation.Rotation90;

		// Now should have green background
		Assert.Equal(greenBrush, label.Background);
	}

	[Fact]
	public void TriggerDeactivatesWhenDetached()
	{
		DeviceDisplay.SetCurrent(new MockDeviceDisplay(
			new(100, 200, 2, DisplayOrientation.Portrait, DisplayRotation.Rotation90)));

		var greenBrush = new SolidColorBrush(Colors.Green);
		var label = new Label();
		var trigger = new DisplayRotationStateTrigger { Rotation = DisplayRotation.Rotation0 };

		VisualStateManager.SetVisualStateGroups(label, new VisualStateGroupList
		{
			new VisualStateGroup
			{
				States =
				{
					new VisualState
					{
						Name = "RotationState",
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
	public void MultipleTriggersWithDifferentRotations()
	{
		DeviceDisplay.SetCurrent(new MockDeviceDisplay(
			new(100, 200, 2, DisplayOrientation.Portrait, DisplayRotation.Rotation180)));

		var redBrush = new SolidColorBrush(Colors.Red);
		var greenBrush = new SolidColorBrush(Colors.Green);
		var blueBrush = new SolidColorBrush(Colors.Blue);
		var yellowBrush = new SolidColorBrush(Colors.Yellow);

		var label = new Label { Background = redBrush };

		VisualStateManager.SetVisualStateGroups(label, new VisualStateGroupList
		{
			new VisualStateGroup
			{
				States =
				{
					new VisualState
					{
						Name = "Rotation0State",
						StateTriggers = { new DisplayRotationStateTrigger { Rotation = DisplayRotation.Rotation0 } },
						Setters = { new Setter { Property = Label.BackgroundProperty, Value = greenBrush } }
					},
					new VisualState
					{
						Name = "Rotation90State",
						StateTriggers = { new DisplayRotationStateTrigger { Rotation = DisplayRotation.Rotation90 } },
						Setters = { new Setter { Property = Label.BackgroundProperty, Value = blueBrush } }
					},
					new VisualState
					{
						Name = "Rotation180State",
						StateTriggers = { new DisplayRotationStateTrigger { Rotation = DisplayRotation.Rotation180 } },
						Setters = { new Setter { Property = Label.BackgroundProperty, Value = yellowBrush } }
					}
				}
			}
		});

		// Should activate Rotation180State (yellow background)
		Assert.Equal(yellowBrush, label.Background);
	}

	[Fact]
	public void DeviceRotationChangeTriggersStateUpdate()
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
						Name = "Rotation90State",
						StateTriggers = { new DisplayRotationStateTrigger { Rotation = DisplayRotation.Rotation90 } },
						Setters = { new Setter { Property = Label.BackgroundProperty, Value = greenBrush } }
					}
				}
			}
		});

		var window = new Window(new ContentPage() { Content = label });

		// Initially should have red background (current Rotation0 != trigger Rotation90)
		Assert.Equal(redBrush, label.Background);

		// Simulate device orientation change to landscape
		mockDeviceDisplay.SetMainDisplayRotation(DisplayRotation.Rotation90);

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

		var trigger = new DisplayRotationStateTrigger { Rotation = DisplayRotation.Rotation90 };
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
						StateTriggers = { new DisplayRotationStateTrigger { Rotation = DisplayRotation.Rotation90 } },
						Setters = { new Setter { Property = Label.BackgroundProperty, Value = greenBrush } }
					}
				}
			}
		});

		var window = new Window(new ContentPage() { Content = label });

		await TestHelpers.Collect();

		mockDeviceDisplay.SetMainDisplayRotation(DisplayRotation.Rotation90);

		Assert.Equal(greenBrush, label.Background);
		GC.KeepAlive(window);
	}
}
