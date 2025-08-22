using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 31190, "[NET10, iOS] SafeArea with SoftInput needs to disable the keyboard auto scroll when there is no scrollview", PlatformAffected.iOS)]
	public partial class Issue31190 : TestContentPage
	{
		public Issue31190()
		{
			InitializeComponent();
		}

		protected override void Init()
		{
			// Configure the page to use SafeArea for keyboard handling
			// This simulates the scenario where SafeArea is configured to handle SoftInput
#if IOS
			On<iOS>().SetUseSafeArea(true);
#endif

			TestEntry.Focused += OnEntryFocused;
			TestEntry.Unfocused += OnEntryUnfocused;
		}

		private void OnEntryFocused(object sender, FocusEventArgs e)
		{
			StatusLabel.Text = "Entry focused - keyboard should show without double insets";
		}

		private void OnEntryUnfocused(object sender, FocusEventArgs e)
		{
			StatusLabel.Text = "Entry unfocused - test completed";
		}
	}
}