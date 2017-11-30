using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 53909, "XML drawables cannot be used as ToolbarItem.Icon ", PlatformAffected.Default)]
	public class Bugzilla53909 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		protected override void Init()
		{

            var tbi = new ToolbarItem();
            tbi.Icon = "synchronize.png";
            tbi.Order = ToolbarItemOrder.Primary;
            tbi.Priority = 0;

            ToolbarItems.Add(tbi);

            // Initialize ui here instead of ctor
            Content = new Label
			{
				Text = "We need to check the icon appears"
			};
		}
	}
}