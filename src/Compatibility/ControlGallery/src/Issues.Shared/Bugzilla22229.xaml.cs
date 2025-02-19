using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if APP
#if UITEST
	[Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 22229, "ToolbarItems not clearing", PlatformAffected.iOS | PlatformAffected.WinPhone, NavigationBehavior.PushAsync)]
	public partial class Bugzilla22229 : TabbedPage
	{
		string _prefix;

		public Bugzilla22229()
		{
			InitializeComponent();
			switch (Device.RuntimePlatform)
			{
				case Device.iOS:
					_prefix = "Images/";
					break;
				case Device.Android:
					_prefix = "";
					break;
				case Device.WinUI:
					_prefix = "Assets/";
					break;
			}
			InitializeActionBar();
		}


		public void OnCurrentPageChanged(object sender, EventArgs e)
		{
			InitializeActionBar();
		}

		void InitializeActionBar()
		{
			ToolbarItems.Clear();
			if (CurrentPage == Children[0])
			{
				ToolbarItems.Add(new ToolbarItem("Action 1", null, () => { }, ToolbarItemOrder.Primary, 1));
				ToolbarItems.Add(new ToolbarItem("Action 2", null, () => { }, ToolbarItemOrder.Primary, 2));

				ToolbarItems.Add(new ToolbarItem("Action 3", null, () => { }, ToolbarItemOrder.Secondary, 3));
				ToolbarItems.Add(new ToolbarItem("Action 4", null, () => { }, ToolbarItemOrder.Secondary, 4));
				ToolbarItems.Add(new ToolbarItem("Action 5", null, () => { }, ToolbarItemOrder.Secondary, 5));
			}

		}
	}
#endif
}
