using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 27715, "ScrollView measures incorrectly when it has AdjustedContentInsets", PlatformAffected.iOS)]
	public partial class Issue27715 : Shell
	{
		public Issue27715()
		{
			InitializeComponent();

		}
	}
}