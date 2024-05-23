using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8263, "[Enhancement] Add On/Off VisualStates for Switch")]
	public partial class Issue8263 : TestContentPage
	{
		public Issue8263()
		{
			InitializeComponent();
		}
		protected override void Init()
		{

		}
	}
}