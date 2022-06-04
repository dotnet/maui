using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class DeviceStateTriggerTests : BaseTestFixture
	{
		[Test]
		public void ConstructionCanHappen()
		{
			var trigger = new DeviceStateTrigger();

			Assert.NotNull(trigger);
		}

		[Test]
		[TestCase("Android", true)]
		[TestCase("iOS", false)]
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

			Assert.That(label.Background, Is.EqualTo(isApplied ? greenBrush : redBrush));
		}
	}
}