#if IOS
using UIKit;
#endif

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 36800, "ScrollView with SafeAreaEdges=\"Container\" reserves the safe area twice, causing phantom scroll range", PlatformAffected.iOS)]
public class Issue36800 : ContentPage
{
	ScrollView _scrollView;
	Label _diagLabel;

	public Issue36800()
	{
		SafeAreaEdges = SafeAreaEdges.None;

		_diagLabel = new Label
		{
			Text = "Phantom=Unknown",
			AutomationId = "DiagLabel"
		};

		var dumpButton = new Button
		{
			Text = "Dump native state",
			AutomationId = "DumpButton"
		};
		dumpButton.Clicked += OnDumpClicked;

		// Small content that comfortably fits within the viewport - the ScrollView
		// should NOT be scrollable at all when the bug is fixed.
		var content = new VerticalStackLayout
		{
			Padding = 16,
			Spacing = 12,
			Children =
			{
				new Label { Text = "Small content", FontSize = 22, AutomationId = "SmallContentLabel" },
				dumpButton,
				_diagLabel
			}
		};

		_scrollView = new ScrollView
		{
			AutomationId = "TestScrollView",
			SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container),
			Content = content
		};

		Content = _scrollView;
	}

	void OnDumpClicked(object sender, EventArgs e)
	{
#if IOS
		if (_scrollView.Handler?.PlatformView is UIScrollView native)
		{
			// Phantom scroll range: on a fixed (non-scrollable) content, the scrollable
			// range should be zero. When the safe area is reserved twice, this value
			// is positive and equals the safe-area thickness.
			double phantom = (native.ContentSize.Height + native.AdjustedContentInset.Top + native.AdjustedContentInset.Bottom) - native.Bounds.Height;
			_diagLabel.Text = $"Phantom={phantom}";
		}
		else
		{
			_diagLabel.Text = "Phantom=NoHandler";
		}
#else
		_diagLabel.Text = "Phantom=0";
#endif
	}
}
