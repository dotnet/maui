using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 33450, "[iOS] Cell with ContextAction has a different layout")]
	public class Bugzilla33450 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		protected override void Init ()
		{
			var list = new ListView { ItemTemplate = new DataTemplate (typeof(MyImageCell)) };

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

			Content = list;
		}

		[Preserve (AllMembers = true)]
		public class MyImageCell : ImageCell
		{
			static bool s_addContextAction = false;

			public MyImageCell()
			{
				ImageSource = "bank.png";
				SetBinding(TextProperty, new Binding("."));
			
				if(s_addContextAction)
				{
					ContextActions.Add(new MenuItem() { Text = "Delete" });
				}
				s_addContextAction = !s_addContextAction;
			}
		}
	}
}
