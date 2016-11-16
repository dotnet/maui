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
	[Issue (IssueTracker.Bugzilla, 34061, "RelativeLayout - First child added after page display does not appear")]
	public class Bugzilla34061 : TestContentPage
	{
		readonly RelativeLayout _layout = new RelativeLayout ();

		protected override void Init ()
		{
			var label = new Label { Text = "Some content goes here", HorizontalOptions = LayoutOptions.Center };

			var addButton = new Button{ Text = "Add Popover", AutomationId = "btnAdd" };
			addButton.Clicked += (s, ea) => AddPopover ();

			var stack = new StackLayout {
				Orientation = StackOrientation.Vertical,
				Children = {
					label,
					addButton
				},
			};

			_layout.Children.Add (stack,
				Forms.Constraint.Constant (0),
				Forms.Constraint.Constant (0),
				Forms.Constraint.RelativeToParent (p => p.Width),
				Forms.Constraint.RelativeToParent (p => p.Height));

			Content = _layout;
		}

		void AddPopover ()
		{
			var newView = new Button {
				BackgroundColor = Color.FromRgba (64, 64, 64, 64),
				Text = "Remove Me",
				AutomationId = "btnRemoveMe"
			};
			newView.Clicked += (s, ea) => RemovePopover (newView);

			_layout.Children.Add (
				newView,
				Forms.Constraint.Constant (0),
				Forms.Constraint.RelativeToParent (p => p.Height / 2),
				Forms.Constraint.RelativeToParent (p => p.Width),
				Forms.Constraint.RelativeToParent (p => p.Height / 2));
		}

		void RemovePopover (View view)
		{
			_layout.Children.Remove (view);
		}

		#if UITEST
		[Test]
		public void Bugzilla34061Test ()
		{
			RunningApp.Screenshot ("I am at Bugzilla34061 ");
			RunningApp.WaitForElement (q => q.Marked ("btnAdd"));
			RunningApp.Tap (q => q.Marked ("btnAdd"));
			RunningApp.WaitForElement (q => q.Marked ("Remove Me"));
			RunningApp.Screenshot ("I see the button");
		}
		#endif
	}
}
