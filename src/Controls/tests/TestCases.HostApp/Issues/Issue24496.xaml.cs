namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 24496, "Pickers scroll to bottom and new keyboard types rekick the scrolling", PlatformAffected.iOS)]
	public partial class Issue24496 : ContentPage
	{
		public Issue24496()
		{
			InitializeComponent();
		}

		void Entry_Focused(object sender, FocusEventArgs e)
		{
#if IOS
			var entry = (Entry)sender;
			var field = entry.Handler?.PlatformView as UIKit.UITextField;
			field?.TintColor = UIKit.UIColor.Clear;
#endif
		}
	}
}