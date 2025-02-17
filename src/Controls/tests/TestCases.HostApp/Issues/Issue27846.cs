namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 27846, "[iOS] More tab doesn't respect shell nav bar customization", PlatformAffected.iOS)]

	public class Issue27846 : TestShell
	{
		protected override void Init()
		{
			SetBackgroundColor(this, Colors.Green);
			AddBottomTab("tab1");
			AddBottomTab("tab2");
			AddBottomTab("tab3");
			AddBottomTab("tab4");
			AddBottomTab("tab5");
			AddBottomTab("tab6");
			AddBottomTab("tab7");
		}
	}
}