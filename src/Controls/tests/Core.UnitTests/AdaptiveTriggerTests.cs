using System;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class AdaptiveTriggerTests : BaseTestFixture
	{
		[Fact]
		public void ResizingWindowPageActivatesTrigger()
		{
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
							Name = "Large",
							StateTriggers = { new AdaptiveTrigger { MinWindowWidth = 300 } },
							Setters = { new Setter { Property = Label.BackgroundProperty, Value = greenBrush } }
						},
						new VisualState
						{
							Name = "Small",
							StateTriggers = { new AdaptiveTrigger { MinWindowWidth = 0 } },
							Setters = { new Setter { Property = Label.BackgroundProperty, Value = blueBrush } }
						}
					}
				}
			});

			var page = new ContentPage
			{
				Frame = new Rect(0, 0, 100, 100),
				Content = label,
			};

			var window = new Window
			{
				Page = page
			};

			label.IsPlatformEnabled = true;
			page.IsPlatformEnabled = true;

			Assert.Equal(label.Background, blueBrush);

			page.Frame = new Rect(0, 0, 500, 100);

			Assert.Equal(label.Background, greenBrush);

			page.Frame = new Rect(0, 0, 100, 100);

			Assert.Equal(label.Background, blueBrush);
		}
	}
}