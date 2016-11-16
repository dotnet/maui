using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue(IssueTracker.Github, 1146, "Disabled Switch in Button Gallery not rendering on all devices", PlatformAffected.Android)]
	public class Issue1146 : TestContentPage
	{
		protected override void Init ()
		{
			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Padding = new Size(20, 20),
					Children = {
						new StackLayout {
							Orientation = StackOrientation.Horizontal,
							Children= {
								new Switch() { IsEnabled = false , AutomationId="switch"},
							},
						},
					}
				}
			};
		} 

#if UITEST
		[Test]
		public void TestSwitchDisable()
		{
			RunningApp.WaitForElement (c => c.Marked ("switch"));
			RunningApp.Screenshot("Is the button here?");
		}
#endif

	}
}
