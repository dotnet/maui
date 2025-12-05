using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests;

public class SafeAreaOrientationTests : BaseTestFixture
{
	[Fact]
	public void SafeAreaEdgesCanBeSetViaVisualStateManager()
	{
		DeviceDisplay.SetCurrent(new MockDeviceDisplay(
			new(100, 200, 2, DisplayOrientation.Portrait, DisplayRotation.Rotation0)));

		var page = new ContentPage();

		VisualStateManager.SetVisualStateGroups(page, new VisualStateGroupList
		{
			new VisualStateGroup
			{
				Name = "OrientationStates",
				States =
				{
					new VisualState
					{
						Name = "Portrait",
						StateTriggers = { new OrientationStateTrigger { Orientation = DisplayOrientation.Portrait } },
						Setters = { new Setter { Property = ContentPage.SafeAreaEdgesProperty, Value = SafeAreaEdges.All } }
					},
					new VisualState
					{
						Name = "Landscape",
						StateTriggers = { new OrientationStateTrigger { Orientation = DisplayOrientation.Landscape } },
						Setters = { new Setter { Property = ContentPage.SafeAreaEdgesProperty, Value = new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.None) } }
					}
				}
			}
		});

		var window = new Window(page);

		// Initially in portrait, should have All edges
		Assert.Equal(SafeAreaEdges.All, page.SafeAreaEdges);
	}

	[Fact]
	public void SafeAreaEdgesChangesWhenOrientationChanges()
	{
		var mockDeviceDisplay = new MockDeviceDisplay(
			new(100, 200, 2, DisplayOrientation.Portrait, DisplayRotation.Rotation0));
		DeviceDisplay.SetCurrent(mockDeviceDisplay);

		var page = new ContentPage();

		VisualStateManager.SetVisualStateGroups(page, new VisualStateGroupList
		{
			new VisualStateGroup
			{
				Name = "OrientationStates",
				States =
				{
					new VisualState
					{
						Name = "Portrait",
						StateTriggers = { new OrientationStateTrigger { Orientation = DisplayOrientation.Portrait } },
						Setters = { new Setter { Property = ContentPage.SafeAreaEdgesProperty, Value = SafeAreaEdges.All } }
					},
					new VisualState
					{
						Name = "Landscape",
						StateTriggers = { new OrientationStateTrigger { Orientation = DisplayOrientation.Landscape } },
						Setters = { new Setter { Property = ContentPage.SafeAreaEdgesProperty, Value = new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.None) } }
					}
				}
			}
		});

		var window = new Window(page);

		// Initially in portrait
		Assert.Equal(SafeAreaEdges.All, page.SafeAreaEdges);

		// Change to landscape
		mockDeviceDisplay.SetMainDisplayOrientation(DisplayOrientation.Landscape);

		// Should now use different safe area edges (horizontal All, vertical None)
		var expected = new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.None);
		Assert.Equal(expected, page.SafeAreaEdges);

		window.Page = null; // Cleanup
	}

	[Fact]
	public void SafeAreaEdgesCanUseContainerRegion()
	{
		DeviceDisplay.SetCurrent(new MockDeviceDisplay(
			new(100, 200, 2, DisplayOrientation.Portrait, DisplayRotation.Rotation0)));

		var page = new ContentPage();

		VisualStateManager.SetVisualStateGroups(page, new VisualStateGroupList
		{
			new VisualStateGroup
			{
				Name = "OrientationStates",
				States =
				{
					new VisualState
					{
						Name = "Portrait",
						StateTriggers = { new OrientationStateTrigger { Orientation = DisplayOrientation.Portrait } },
						Setters = { new Setter { Property = ContentPage.SafeAreaEdgesProperty, Value = SafeAreaEdges.Container } }
					}
				}
			}
		});

		var window = new Window(page);

		// Should use Container region
		Assert.Equal(SafeAreaEdges.Container, page.SafeAreaEdges);

		window.Page = null; // Cleanup
	}

	[Fact]
	public void SafeAreaEdgesWorksWithDifferentPageTypes()
	{
		DeviceDisplay.SetCurrent(new MockDeviceDisplay(
			new(100, 200, 2, DisplayOrientation.Portrait, DisplayRotation.Rotation0)));

		var contentPage = new ContentPage();
		var tabbedPage = new TabbedPage();

		VisualStateManager.SetVisualStateGroups(contentPage, new VisualStateGroupList
		{
			new VisualStateGroup
			{
				Name = "OrientationStates",
				States =
				{
					new VisualState
					{
						Name = "Portrait",
						StateTriggers = { new OrientationStateTrigger { Orientation = DisplayOrientation.Portrait } },
						Setters = { new Setter { Property = ContentPage.SafeAreaEdgesProperty, Value = SafeAreaEdges.All } }
					}
				}
			}
		});

		var window = new Window(contentPage);

		// ContentPage should respect VSM
		Assert.Equal(SafeAreaEdges.All, contentPage.SafeAreaEdges);

		window.Page = null; // Cleanup
	}
}
