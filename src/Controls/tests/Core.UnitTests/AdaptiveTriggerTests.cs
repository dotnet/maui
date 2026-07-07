using System;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class AdaptiveTriggerTests : BaseTestFixture
	{
		// Regression tests for https://github.com/dotnet/maui/issues/36032
		// AdaptiveTrigger leaks VisualElement when VisualStateGroups are replaced after attach

		[Fact]
		public void OldAdaptiveTriggerDetachedWhenVSGsReplacedAfterAttach()
		{
			// Arrange: set up a label with an AdaptiveTrigger, attach it to a Window
			var label = new Label();
			var oldTrigger = new AdaptiveTrigger { MinWindowWidth = 300 };
			var newTrigger = new AdaptiveTrigger { MinWindowWidth = 500 };

			VisualStateManager.SetVisualStateGroups(label, new VisualStateGroupList
			{
				new VisualStateGroup
				{
					States =
					{
						new VisualState
						{
							Name = "Large",
							StateTriggers = { oldTrigger },
							Setters = { new Setter { Property = Label.BackgroundProperty, Value = new SolidColorBrush(Colors.Green) } }
						},
					}
				}
			});

			var page = new ContentPage { Content = label };
			_ = new Window { Page = page };

			// Old trigger should be attached after element joins a Window
			Assert.True(oldTrigger.IsAttached);

			// Act: replace the VisualStateGroups while the element is already attached to a Window
			VisualStateManager.SetVisualStateGroups(label, new VisualStateGroupList
			{
				new VisualStateGroup
				{
					States =
					{
						new VisualState
						{
							Name = "ExtraLarge",
							StateTriggers = { newTrigger },
							Setters = { new Setter { Property = Label.BackgroundProperty, Value = new SolidColorBrush(Colors.Blue) } }
						},
					}
				}
			});

			// Assert: old trigger must be detached (unsubscribed from Window.SizeChanged)
			Assert.False(oldTrigger.IsAttached, "Old AdaptiveTrigger should be detached after VSGs are replaced");
			// Assert: new trigger should now be attached
			Assert.True(newTrigger.IsAttached, "New AdaptiveTrigger should be attached after VSGs are replaced");
		}

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

			IWindow window = new Window
			{
				Page = page
			};

			window.FrameChanged(new Rect(0, 0, 100, 100));

			Assert.Equal(label.Background, blueBrush);

			window.FrameChanged(new Rect(0, 0, 500, 100));

			Assert.Equal(label.Background, greenBrush);

			window.FrameChanged(new Rect(0, 0, 100, 100));

			Assert.Equal(label.Background, blueBrush);
		}


		[Fact]
		public void ValidateAdaptiveTriggerDisconnects()
		{
			var greenBrush = new SolidColorBrush(Colors.Green);
			var label = new Label();
			var trigger = new AdaptiveTrigger { MinWindowWidth = 300 };


			VisualStateManager.SetVisualStateGroups(label, new VisualStateGroupList
			{
				new VisualStateGroup
				{
					States =
					{
						new VisualState
						{
							Name = "Large",
							StateTriggers = { trigger },
							Setters = { new Setter { Property = Label.BackgroundProperty, Value = greenBrush } }
						},
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
	}
}