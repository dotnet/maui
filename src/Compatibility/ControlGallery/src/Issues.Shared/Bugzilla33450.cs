using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 33450, "[iOS] Cell with ContextAction has a different layout")]
	public class Bugzilla33450 : TestContentPage // or TestFlyoutPage, etc ...
	{
		protected override void Init()
		{
			var list = new ListView { ItemTemplate = new DataTemplate(typeof(MyImageCell)) };

			list.ItemsSource = new[] {
				"One",
				"Two",
				"Three",
				"Four",
				"Five",
				"Six",
				"Seven",
				"Eight",
				"Nine",
				"Ten",
			};

			Content = new StackLayout
			{
				Children = {
					new Label { Text = "The following list contains cells with and without context actions, but all of the cells should be laid out identically. If the cells do not look identical, this test has failed." },
					list }
			};
		}

		[Preserve(AllMembers = true)]
		public class MyImageCell : ImageCell
		{
			static bool s_addContextAction = false;

			public MyImageCell()
			{
				ImageSource = "bank.png";
				SetBinding(TextProperty, new Binding("."));

				if (s_addContextAction)
				{
					ContextActions.Add(new MenuItem() { Text = "Delete" });
				}
				s_addContextAction = !s_addContextAction;
			}
		}
	}
}
