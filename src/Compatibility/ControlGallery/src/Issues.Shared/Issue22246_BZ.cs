using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 22246, "Entry in Grid nested in ViewCell isn't expanding", PlatformAffected.WinPhone, NavigationBehavior.PushModalAsync)]
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.TableView)]
#endif
	public class Issue22246Bz : ContentPage
	{
		public Issue22246Bz()
		{
			var table = new TableView
			{
				Root = new TableRoot {
					new TableSection("Testing Section") {
						new ViewCell {
							View = CreateBugView()
						}
					}
				}
			};

			var layout = new StackLayout
			{
				Children = {
					CreateBugView(),
					table
				}
			};

			Content = layout;
		}

		View CreateBugView()
		{
			return new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				Spacing = 10,
				Children = {
					new Entry {Placeholder = "Entry", HorizontalOptions = LayoutOptions.FillAndExpand},
					new Button {Text = "Button"}
				}
			};
		}
	}
}
