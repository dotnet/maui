using System;
using System.Threading.Tasks;
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

		[Fact]
		public void ReplacingVisualStateGroupsPreservesReusedAdaptiveTrigger()
		{
			var redBrush = new SolidColorBrush(Colors.Red);
			var greenBrush = new SolidColorBrush(Colors.Green);
			var label = new Label { Background = redBrush };
			var trigger = new AdaptiveTrigger { MinWindowWidth = 300 };

			VisualStateManager.SetVisualStateGroups(label, new VisualStateGroupList
			{
				new VisualStateGroup
				{
					States =
					{
						new VisualState
						{
							Name = "OriginalLarge",
							StateTriggers = { trigger },
						},
					}
				}
			});

			var page = new ContentPage { Content = label };
			IWindow window = new Window { Page = page };
			window.FrameChanged(new Rect(0, 0, 100, 100));

			Assert.True(trigger.IsAttached);
			Assert.Equal(redBrush, label.Background);

			VisualStateManager.SetVisualStateGroups(label, new VisualStateGroupList
			{
				new VisualStateGroup
				{
					States =
					{
						new VisualState
						{
							Name = "ReplacementLarge",
							StateTriggers = { trigger },
							Setters = { new Setter { Property = Label.BackgroundProperty, Value = greenBrush } },
						},
					}
				}
			});

			Assert.True(trigger.IsAttached);

			window.FrameChanged(new Rect(0, 0, 500, 100));

			Assert.Equal(greenBrush, label.Background);
		}

		[Fact]
		public async Task ReplacingVisualStateGroupsDoesNotLeakVisualElement()
		{
			// A long-lived window that stays alive for the duration of the test.
			var page = new ContentPage();

			WeakReference weakElement;
			Window window;
			{
				var label = new Label();

				// Attach a VisualStateGroupList whose trigger subscribes to Window.SizeChanged
				// (a strong event) and holds the VisualElement strongly. The groups must be set
				// before the element is attached to a window so the trigger gets attached.
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
							},
						}
					}
				});

				page.Content = label;
				window = new Window { Page = page };

				// Replace the groups while the element is attached to the window. The old
				// trigger must be detached, otherwise it stays subscribed to Window.SizeChanged
				// and keeps the VisualElement alive.
				VisualStateManager.SetVisualStateGroups(label, new VisualStateGroupList());

				// Remove the element from the tree so nothing else roots it.
				page.Content = new Label();

				weakElement = new WeakReference(label);
			}

			Assert.False(await weakElement.WaitForCollect(), "VisualElement should not be alive!");
			GC.KeepAlive(window);
		}
	}
}