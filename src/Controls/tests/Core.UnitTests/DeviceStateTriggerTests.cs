// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class DeviceStateTriggerTests : BaseTestFixture
	{
		[Fact]
		public void ConstructionCanHappen()
		{
			var trigger = new DeviceStateTrigger();

			Assert.NotNull(trigger);
		}

		[Theory]
		[InlineData("Android", true)]
		[InlineData("iOS", false)]
		public void CorrectStateIsAppliedWhenAttached(string triggerDevice, bool isApplied)
		{
			DeviceInfo.SetCurrent(new MockDeviceInfo(platform: DevicePlatform.Android));

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
							Name = "AndroidThings",
							StateTriggers = { new DeviceStateTrigger { Device = triggerDevice } },
							Setters = { new Setter { Property = Label.BackgroundProperty, Value = greenBrush } }
						}
					}
				}
			});

			label.IsPlatformEnabled = true;

			Assert.Equal(label.Background, isApplied ? greenBrush : redBrush);
		}
	}
}