using System;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 34061, "RelativeLayout - First child added after page display does not appear")]
	public class Bugzilla34061 : TestContentPage
	{
		Compatibility.RelativeLayout _layout;

		protected override void Init()
		{
			_layout = new Compatibility.RelativeLayout();
			var label = new Label { Text = "Some content goes here", HorizontalOptions = LayoutOptions.Center };

			var addButton = new Button { Text = "Add Popover", AutomationId = "btnAdd" };
			addButton.Clicked += (s, ea) => AddPopover();

			var stack = new StackLayout
			{
				Orientation = StackOrientation.Vertical,
				Children = {
					label,
					addButton
				},
			};

			_layout.Children.Add(stack,
				Compatibility.Constraint.Constant(0),
				Compatibility.Constraint.Constant(0),
				Compatibility.Constraint.RelativeToParent(p => p.Width),
				Compatibility.Constraint.RelativeToParent(p => p.Height));

			Content = _layout;
		}

		void AddPopover()
		{
			var newView = new Button
			{
				BackgroundColor = Color.FromRgba(64, 64, 64, 64),
				Text = "Remove Me",
				AutomationId = "btnRemoveMe"
			};
			newView.Clicked += (s, ea) => RemovePopover(newView);

			_layout.Children.Add(
				newView,
				Compatibility.Constraint.Constant(0),
				Compatibility.Constraint.RelativeToParent(p => p.Height / 2),
				Compatibility.Constraint.RelativeToParent(p => p.Width),
				Compatibility.Constraint.RelativeToParent(p => p.Height / 2));
		}

		void RemovePopover(View view)
		{
			_layout.Children.Remove(view);
		}

#if UITEST
[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiAndroid]
		[Test]
		[FailsOnMaui]
		public void Bugzilla34061Test()
		{
			RunningApp.Screenshot("I am at Bugzilla34061 ");
			RunningApp.WaitForElement(q => q.Marked("btnAdd"));
			RunningApp.Tap(q => q.Marked("btnAdd"));
			RunningApp.WaitForElement(q => q.Marked("Remove Me"));
			RunningApp.Screenshot("I see the button");
		}
#endif
	}
}
