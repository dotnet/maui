using System.Reflection;
using Maui.Controls.Sample.Issues;

namespace Maui.Controls.Sample.Issues;

[Issue(	IssueTracker.Github,
		27998,
		"[Windows] ScrollView is not scrolling to the bottom if in grid with *,auto Width",
		PlatformAffected.UWP)]
public partial class Issue27998 : TestContentPage
{
	public Issue27998()
	{
		InitializeComponent();
	}

	protected override void Init() { }

}
