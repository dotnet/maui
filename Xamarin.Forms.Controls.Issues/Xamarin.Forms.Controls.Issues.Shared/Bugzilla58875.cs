using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Collections.ObjectModel;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.ListView)]
	[Category(UITestCategories.ContextActions)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 58875, "Back navigation disables Context Action in whole app, if Context Action left open", PlatformAffected.iOS)]
	public class Bugzilla58875 : TestNavigationPage
	{
		const string Button1Id = "Button1Id";
		const string ContextAction = "More";
		const string Target = "Swipe me";

		protected override void Init()
		{
			var page1 = new Page1();
			Navigation.PushAsync(page1);
		}

		[Preserve(AllMembers = true)]
		class ListViewPage : ContentPage
		{
			public ListViewPage()
			{
				BindingContext = this;

				var listView = new ListView(ListViewCachingStrategy.RecycleElement)
				{
					ItemTemplate = new DataTemplate(() =>
					{
						var label = new Label { };
						label.SetBinding(Label.TextProperty, ".");
						var viewcell = new ViewCell
						{
							View = new StackLayout { Children = { label } }
						};
						viewcell.ContextActions.Add(new MenuItem { Text = ContextAction });
						viewcell.ContextActions.Add(new MenuItem { Text = "Delete", IsDestructive = true });
						return viewcell;
					})
				};

				listView.SetBinding(ListView.ItemsSourceProperty, nameof(Items));

				Items = new ObservableCollection<string> {
						"Item 1",
						Target,
						"Item 3",
						"Swipe me too, leave me open",
						"Swipe left -> right (trigger back navigation)"
				};

				Content = listView;
			}

			public ObservableCollection<string> Items { get; set; }
		}

		[Preserve(AllMembers = true)]
		class Page1 : ContentPage
		{
			public Page1()
			{
				var button = new Button { Text = "Tap me", AutomationId = Button1Id };
				button.Clicked += Button_Clicked;
				Content = button;
			}

			void Button_Clicked(object sender, System.EventArgs e)
			{
				var listPage = new ListViewPage();
				Navigation.PushAsync(listPage);
			}
		}

#if UITEST && __IOS__
		[Test]
		public void Bugzilla58875Test()
		{
			RunningApp.WaitForElement(q => q.Marked(Button1Id));
			RunningApp.Tap(q => q.Marked(Button1Id));
			RunningApp.WaitForElement(q => q.Marked(Target));
			RunningApp.ActivateContextMenu(Target);
			RunningApp.WaitForElement(q => q.Marked(ContextAction));
			RunningApp.Back();

#if __ANDROID__
			RunningApp.Back(); // back button dismisses the ContextAction first, so we need to hit back one more time to get to previous page
#endif

			RunningApp.WaitForElement(q => q.Marked(Button1Id));
			RunningApp.Tap(q => q.Marked(Button1Id));
			RunningApp.WaitForElement(q => q.Marked(Target));
			RunningApp.ActivateContextMenu(Target);
			RunningApp.WaitForElement(q => q.Marked(ContextAction));
		}
#endif
	}
}