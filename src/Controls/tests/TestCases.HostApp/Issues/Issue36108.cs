using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues;

// Regression guard for #36108 — Android Shell handler OnDestroyView defensive cleanup.
// Tests that after switching between FlyoutItems:
//   1. Toolbar title is correctly configured (UpdateDisplayedPage ran — not early-returned on stale _displayedPage)
//   2. Tab switching still updates the toolbar (ViewPager2 page callback registered correctly)
[Issue(IssueTracker.Github, 36108, "Android Shell handler — OnDestroyView defensive cleanup regression guard", PlatformAffected.Android)]
public class Issue36108 : TestShell
{
	public const string FlyoutItemATitle = "Section A";
	public const string FlyoutItemBTitle = "Section B";
	public const string Tab1Title = "Tab 1";
	public const string Tab2Title = "Tab 2";
	public const string Tab1LabelId = "Tab1Label";
	public const string Tab2LabelId = "Tab2Label";
	public const string SectionBLabelId = "SectionBLabel";

	protected override void Init()
	{
		var tab1Page = new ContentPage
		{
			Title = Tab1Title,
			Content = new Label
			{
				Text = "Tab 1 Content",
				AutomationId = Tab1LabelId,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			}
		};

		var tab2Page = new ContentPage
		{
			Title = Tab2Title,
			Content = new Label
			{
				Text = "Tab 2 Content",
				AutomationId = Tab2LabelId,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			}
		};

		var sectionBPage = new ContentPage
		{
			Title = FlyoutItemBTitle,
			Content = new Label
			{
				Text = "Section B Content",
				AutomationId = SectionBLabelId,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			}
		};

		// FlyoutItem A — has two bottom tabs
		var flyoutItemA = new FlyoutItem
		{
			Title = FlyoutItemATitle,
			Route = "SectionA",
			Items =
			{
				new Tab
				{
					Title = Tab1Title,
					Route = "Tab1",
					AutomationId = Tab1Title,
					Items = { new ShellContent { Content = tab1Page } }
				},
				new Tab
				{
					Title = Tab2Title,
					Route = "Tab2",
					AutomationId = Tab2Title,
					Items = { new ShellContent { Content = tab2Page } }
				}
			}
		};

		// FlyoutItem B — simple page
		var flyoutItemB = new FlyoutItem
		{
			Title = FlyoutItemBTitle,
			Route = "SectionB",
			Items =
			{
				new Tab
				{
					Items = { new ShellContent { Content = sectionBPage } }
				}
			}
		};

		Items.Add(flyoutItemA);
		Items.Add(flyoutItemB);
	}
}
