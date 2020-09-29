using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 49304, "[UWP] ScrollView and ListView are not scrolling after rotation", PlatformAffected.UWP)]
	public class Bugzilla49304 : TestContentPage
	{
		protected override void Init()
		{
			var stack = new StackLayout();
			for (int i = 0; i < 50; i++)
			{
				stack.Children.Add(new Label { Text = i.ToString() });
			}

			var items = new List<string>();
			for (int i = 0; i < 50; i++)
			{
				items.Add(i.ToString());
			}
			Content = new StackLayout
			{
				Children =
				{
					new Label {
						Text = "The ScrollView, ListView, and TableView below should be rotated 180 degrees, but only the latter two " +
							   "should still be scrollable, as the ScrollView has non-zero (.000001/-.000001 RotationX/RotationY values."
					},
					new ScrollView
					{
						Content = stack,
						Rotation = 180,
						RotationX = .000001,
						RotationY = -.000001
					},
					new ListView
					{
						ItemsSource = items,
						Rotation = 180,
						TranslationX = -20
					},
					new TableView {
						Rotation = 180,
						Intent = TableIntent.Form,
						Root = new TableRoot ("Table Title") {
							new TableSection ("Section 1 Title") {
								new TextCell {
									Text = "TextCell Text",
									Detail = "TextCell Detail"
								},
								new EntryCell {
									Label = "EntryCell:",
									Placeholder = "default keyboard",
									Keyboard = Keyboard.Default
								}
							},
							new TableSection ("Section 2 Title") {
								new EntryCell {
									Label = "Another EntryCell:",
									Placeholder = "phone keyboard",
									Keyboard = Keyboard.Telephone
								},
								new SwitchCell {
									Text = "SwitchCell:"
								}
							}
						}
					}
				}
			};
		}
	}
}