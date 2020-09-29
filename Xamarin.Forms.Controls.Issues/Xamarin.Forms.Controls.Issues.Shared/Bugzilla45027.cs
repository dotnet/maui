using System.Collections.Generic;
using System.Linq;
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
	[Issue(IssueTracker.Bugzilla, 45027, "App crashes when double tapping on ToolbarItem or MenuItem very quickly", PlatformAffected.Android)]
	public class Bugzilla45027 : TestContentPage // or TestFlyoutPage, etc ...
	{
		const string BUTTON_ACTION_TEXT = "Action";
		const string BUTTON_DELETE_TEXT = "Delete";

		List<int> _list;
		public List<int> List
		{
			get
			{
				if (_list == null)
				{
					_list = new List<int>();
					for (var i = 0; i < 10; i++)
						_list.Add(i);
				}

				return _list;
			}
		}

		protected override void Init()
		{
			var stackLayout = new StackLayout
			{
				Orientation = StackOrientation.Vertical,
				Children =
				{
					new Label
					{
						Text = "Long tap list items to display context menu. Double tapping each action rapidly should not crash.",
						HorizontalTextAlignment = TextAlignment.Center
					}
				}
			};

			var listView = new ListView
			{
				ItemsSource = List,
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label();
					label.SetBinding(Label.TextProperty, new Binding("."));

					return new ViewCell
					{
						View = new ContentView
						{
							Content = label,
						},
						ContextActions = { new MenuItem
						{
							Text = BUTTON_ACTION_TEXT
						},
						new MenuItem
						{
							Text = BUTTON_DELETE_TEXT,
							IsDestructive = true
						} }
					};
				})
			};
			stackLayout.Children.Add(listView);

			Content = stackLayout;
		}

#if UITEST && __ANDROID__
		[Test]
		public void Bugzilla45027Test()
		{
			var firstItemList = List.First().ToString();
			RunningApp.WaitForElement(q => q.Marked(firstItemList));

			RunningApp.TouchAndHold(q => q.Marked(firstItemList));
			RunningApp.WaitForElement(q => q.Marked(BUTTON_ACTION_TEXT));
			RunningApp.DoubleTap(q => q.Marked(BUTTON_ACTION_TEXT));

			RunningApp.TouchAndHold(q => q.Marked(firstItemList));
			RunningApp.WaitForElement(q => q.Marked(BUTTON_DELETE_TEXT));
			RunningApp.DoubleTap(q => q.Marked(BUTTON_DELETE_TEXT));
		}
#endif

#if UITEST && __IOS__
		[Test]
		public void Bugzilla45027Test()
		{
			var firstItemList = List.First().ToString();
			RunningApp.WaitForElement(q => q.Marked(firstItemList));

			RunningApp.SwipeRightToLeft(q => q.Marked(firstItemList));
			RunningApp.WaitForElement(q => q.Marked(BUTTON_ACTION_TEXT));
			RunningApp.DoubleTap(q => q.Marked(BUTTON_ACTION_TEXT));

			RunningApp.SwipeRightToLeft(q => q.Marked(firstItemList));
			RunningApp.WaitForElement(q => q.Marked(BUTTON_DELETE_TEXT));
			RunningApp.DoubleTap(q => q.Marked(BUTTON_DELETE_TEXT));
		}
#endif
	}
}