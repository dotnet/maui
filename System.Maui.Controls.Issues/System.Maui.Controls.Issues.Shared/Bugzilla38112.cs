using System;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 38112, "Switch becomes reenabled when previous ViewCell is removed from TableView", PlatformAffected.Android)]
	public class Bugzilla38112 : TestContentPage
	{
		bool _removed;
		protected override void Init ()
		{
			var layout = new StackLayout ();
			var button = new Button { Text = "Click" };
			var tablesection = new TableSection { Title = "Switches" };
			var tableview = new TableView { Intent = TableIntent.Form, Root = new TableRoot { tablesection } };
			var viewcell1 = new ViewCell {
				View = new StackLayout {
					HorizontalOptions = LayoutOptions.FillAndExpand,
					Orientation = StackOrientation.Horizontal,
					Children = {
						new Label { Text = "Switch 1", HorizontalOptions = LayoutOptions.StartAndExpand },
						new Switch { AutomationId = "switch1", HorizontalOptions = LayoutOptions.End, IsToggled = true }
					}
				}
			};
			var viewcell2 = new ViewCell {
				View = new StackLayout {
					HorizontalOptions = LayoutOptions.FillAndExpand,
					Orientation = StackOrientation.Horizontal,
					Children = {
						new Label { Text = "Switch 2", HorizontalOptions = LayoutOptions.StartAndExpand },
						new Switch { AutomationId = "switch2", HorizontalOptions = LayoutOptions.End, IsToggled = true }
					}
				}
			};
			Label label = new Label { Text = "Switch 3", HorizontalOptions = LayoutOptions.StartAndExpand };
			Switch switchie = new Switch { AutomationId = "switch3", HorizontalOptions = LayoutOptions.End, IsToggled = true, IsEnabled = false };
			switchie.Toggled += (sender, e) => {
				label.Text = "FAIL";
			};
			var viewcell3 = new ViewCell {
				View = new StackLayout {
					HorizontalOptions = LayoutOptions.FillAndExpand,
					Orientation = StackOrientation.Horizontal,
					Children = {
						label,
						switchie,
					}
				}
			};

			tablesection.Add (viewcell1);
			tablesection.Add (viewcell2);
			tablesection.Add (viewcell3);

			button.Clicked += (sender, e) => {
				if (_removed)
					tablesection.Insert (1, viewcell2);
				else
					tablesection.Remove (viewcell2);

				_removed = !_removed;
			};

			layout.Children.Add (button);
			layout.Children.Add (tableview);

			Content = layout;
		}

#if UITEST
		[Test]
		public void Bugzilla38112_SwitchIsStillOnScreen ()
		{
			RunningApp.WaitForElement (q => q.Marked ("Click"));
			RunningApp.Tap (q => q.Marked ("Click"));
			RunningApp.WaitForElement (q => q.Marked ("switch3"));
		}

		[Test]
		public void Bugzilla38112_SwitchIsStillDisabled ()
		{
			RunningApp.WaitForElement (q => q.Marked ("Click"));
			RunningApp.Tap (q => q.Marked ("Click"));
			RunningApp.WaitForElement (q => q.Marked ("switch3"));
			RunningApp.Tap (q => q.Marked ("switch3"));
			RunningApp.WaitForNoElement (q => q.Marked ("FAIL"));
		}
#endif
	}
}
