using System;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.TableView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 36559, "[WP] Navigating to a ContentPage with a Grid inside a TableView affects Entry heights")]
	public class Bugzilla36559 : TestContentPage
	{
		protected override void Init()
		{
			var label = new Label { Text = "Label" };
			var entry = new Entry { AutomationId = "entry" };
			var grid = new Grid();

			grid.Children.Add(label, 0, 0);
			grid.Children.Add(entry, 1, 0);
			var tableView = new TableView
			{
				Root = new TableRoot
				{
					new TableSection
					{
						new ViewCell
						{
							View = grid
						}
					}
				}
			};

			Content = new StackLayout
			{
				Children = { tableView }
			};
		}

#if UITEST
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void Bugzilla36559Test()
		{
			RunningApp.WaitForElement(q => q.Marked("entry"));
			var results = RunningApp.Query(q => q.Marked("entry"));
			Assert.AreNotEqual(results[0].Rect.Height, -1);
		}
#endif
	}
}
