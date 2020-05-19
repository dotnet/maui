using System;
using System.Collections.ObjectModel;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
    [Preserve(AllMembers = true)]
    [Issue(IssueTracker.Bugzilla, 56710, "ContextActionsCell.OnMenuItemPropertyChanged throws NullReferenceException", PlatformAffected.iOS)]
    public class Bugzilla56710 : TestNavigationPage
    {
        protected override void Init()
        {
            var root = new ContentPage
            {
                Content = new Button
                {
                    Text = "Go to Test Page",
                    Command = new Command(() => PushAsync(new TestPage()))
                }
            };

            PushAsync(root);
        }

#if UITEST
        [Test]
        public void Bugzilla56710Test ()
        {
            RunningApp.WaitForElement (q => q.Marked ("Go to Test Page"));
            RunningApp.Tap (q => q.Marked ("Go to Test Page"));

            RunningApp.WaitForElement(q => q.Marked("Item 3"));
            RunningApp.Back();
        }
#endif
	}

	[Preserve(AllMembers = true)]
	public class TestPage : ContentPage
	{
		ObservableCollection<TestItem> Items;

		public TestPage()
		{
			Items = new ObservableCollection<TestItem>();
			Items.Add(new TestItem { Text = "Item 1", ItemText = "Action 1" });
			Items.Add(new TestItem { Text = "Item 2", ItemText = "Action 2" });
			Items.Add(new TestItem { Text = "Item 3", ItemText = "Action 3" });

			var testListView = new ListView
			{
				ItemsSource = Items,
				ItemTemplate = new DataTemplate(typeof(TestCell))
			};

			Content = testListView;
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();

			Items.Clear();
		}
	}

	[Preserve(AllMembers = true)]
	public class TestItem
	{
		public string Text { get; set; }
		public string ItemText { get; set; }
	}

	[Preserve(AllMembers = true)]
	public class TestCell : ViewCell
	{
		public TestCell()
		{
			var menuItem = new MenuItem();
			menuItem.SetBinding(MenuItem.TextProperty, "ItemText");
			ContextActions.Add(menuItem);


			var textLabel = new Label();
			textLabel.SetBinding(Label.TextProperty, "Text");
			View = textLabel;
		}
	}
}