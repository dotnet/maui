using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
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
			var mockDeviceDisplay = new MockDeviceDisplay();
			var displayInfo = new DisplayInfo(
				100, 200, 2, DisplayOrientation.Portrait, currentRotation);
			mockDeviceDisplay.UpdateMainDisplayInfo(displayInfo);
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
							Name = "RotationState",
							StateTriggers = { new DisplayRotationStateTrigger { Rotation = triggerRotation } },
							Setters = { new Setter { Property = Label.BackgroundProperty, Value = greenBrush } }
						}
					}
				}
			});

			label.IsPlatformEnabled = true;

			Assert.Equal(label.Background, isApplied ? greenBrush : redBrush);
		}

		[Fact]
		public void StateChangesWhenRotationChanges()
		{
			var mockDeviceDisplay = new MockDeviceDisplay();
			var initialDisplayInfo = new DisplayInfo(
				100, 200, 2, DisplayOrientation.Portrait, DisplayRotation.Rotation0);
			mockDeviceDisplay.UpdateMainDisplayInfo(initialDisplayInfo);
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

			label.IsPlatformEnabled = true;

			// Initially should have red background (Rotation0 != Rotation90)
			Assert.Equal(redBrush, label.Background);

			// Change to Rotation90 - should trigger state change
			mockDeviceDisplay.SetMainDisplayRotation(DisplayRotation.Rotation90);

			// Now should have green background
			Assert.Equal(greenBrush, label.Background);

			// Change back to Rotation0 - should revert to default state
			mockDeviceDisplay.SetMainDisplayRotation(DisplayRotation.Rotation0);

			// Should revert to red background
			Assert.Equal(redBrush, label.Background);
		}

		[Fact]
		public void RotationPropertyChangeTriggersStateUpdate()
		{
			var mockDeviceDisplay = new MockDeviceDisplay();
			var displayInfo = new DisplayInfo(
				100, 200, 2, DisplayOrientation.Portrait, DisplayRotation.Rotation90);
			mockDeviceDisplay.UpdateMainDisplayInfo(displayInfo);
			DeviceDisplay.SetCurrent(mockDeviceDisplay);

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

			label.IsPlatformEnabled = true;

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
			var mockDeviceDisplay = new MockDeviceDisplay();
			var displayInfo = new DisplayInfo(
				100, 200, 2, DisplayOrientation.Portrait, DisplayRotation.Rotation0);
			mockDeviceDisplay.UpdateMainDisplayInfo(displayInfo);
			DeviceDisplay.SetCurrent(mockDeviceDisplay);

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

			var page = new ContentPage
			{
				Content = label,
			};

			Assert.False(trigger.IsAttached);
			
			_ = new Window
			{
				Page = page
			};

			Assert.True(trigger.IsAttached);

			page.Content = new Label();

			Assert.False(trigger.IsAttached);
		}

		[Theory]
		[InlineData(DisplayRotation.Rotation0)]
		[InlineData(DisplayRotation.Rotation90)]
		[InlineData(DisplayRotation.Rotation180)]
		[InlineData(DisplayRotation.Rotation270)]
		[InlineData(DisplayRotation.Unknown)]
		public void WorksWithAllRotationValues(DisplayRotation rotation)
		{
			var mockDeviceDisplay = new MockDeviceDisplay();
			var displayInfo = new DisplayInfo(
				100, 200, 2, DisplayOrientation.Portrait, rotation);
			mockDeviceDisplay.UpdateMainDisplayInfo(displayInfo);
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
							Name = "RotationState",
							StateTriggers = { new DisplayRotationStateTrigger { Rotation = rotation } },
							Setters = { new Setter { Property = Label.BackgroundProperty, Value = greenBrush } }
						}
					}
				}
			});

			label.IsPlatformEnabled = true;

			// Should activate since current rotation matches trigger rotation
			Assert.Equal(greenBrush, label.Background);
		}

		[Fact]
		public void MultipleTriggersWithDifferentRotations()
		{
			var mockDeviceDisplay = new MockDeviceDisplay();
			var displayInfo = new DisplayInfo(
				100, 200, 2, DisplayOrientation.Portrait, DisplayRotation.Rotation180);
			mockDeviceDisplay.UpdateMainDisplayInfo(displayInfo);
			DeviceDisplay.SetCurrent(mockDeviceDisplay);

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

			label.IsPlatformEnabled = true;

			// Should activate Rotation180State (yellow background)
			Assert.Equal(yellowBrush, label.Background);
		}
	}
}