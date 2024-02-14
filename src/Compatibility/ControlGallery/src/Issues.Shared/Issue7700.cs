using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.CollectionView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7700, "[Bug][iOS] If CollectionView in other Tab gets changed before it's displayed, it stays invisible",
		PlatformAffected.iOS)]
	public class Issue7700 : TestTabbedPage
	{
		readonly ObservableCollection<string> _source = new ObservableCollection<string>() { "one", "two", "three" };
		readonly ObservableCollection<Group> _groupedSource = new ObservableCollection<Group>();

		[Preserve(AllMembers = true)]
		class Group : List<string>
		{
			public string Text { get; set; }

			public Group()
			{
				Add("Uno");
				Add("Dos");
				Add("Tres");
			}
		}

		const string Add1 = "Add1";
		const string Add2 = "Add2";
		const string Success = "Success";
		const string Tab2 = "Tab2";
		const string Tab3 = "Tab3";
		const string Add1Label = "Add to List";
		const string Add2Label = "Add to Grouped List";

		protected override void Init()
		{
#if APP
			Children.Add(FirstPage());
			Children.Add(CollectionViewPage());
			Children.Add(GroupedCollectionViewPage());
#endif
		}

		ContentPage FirstPage()
		{
			var page = new ContentPage() { Title = "7700 First Page", Padding = 40 };

			var instructions = new Label { Text = $"Tap the button marked '{Add1Label}'. Then tap the button marked '{Add2Label}'. If the application does not crash, the test has passed." };

			var button1 = new Button() { Text = Add1Label, AutomationId = Add1 };
			button1.Clicked += Button1Clicked;

			var button2 = new Button() { Text = Add2Label, AutomationId = Add2 };
			button2.Clicked += Button2Clicked;

			var layout = new StackLayout { Children = { instructions, button1, button2 } };

			page.Content = layout;

			return page;
		}

		void Button1Clicked(object sender, EventArgs e)
		{
			_source.Insert(0, Success);
		}

		void Button2Clicked(object sender, EventArgs e)
		{
			_groupedSource.Insert(0, new Group() { Text = Success });
		}

		ContentPage CollectionViewPage()
		{
			var cv = new CollectionView
			{
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label();
					label.SetBinding(Label.TextProperty, new Binding("."));
					return label;
				}),

				ItemsSource = _source
			};

			var page = new ContentPage() { Title = Tab2, Padding = 40 };

			page.Content = cv;

			return page;
		}

		ContentPage GroupedCollectionViewPage()
		{
			var cv = new CollectionView
			{
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label();
					label.SetBinding(Label.TextProperty, new Binding("."));
					return label;
				}),

				GroupHeaderTemplate = new DataTemplate(() =>
				{
					var label = new Label();
					label.SetBinding(Label.TextProperty, new Binding("Text"));
					return label;
				}),

				GroupFooterTemplate = new DataTemplate(() =>
				{
					var label = new Label();
					label.SetBinding(Label.TextProperty, new Binding("Text"));
					return label;
				}),

				ItemsSource = _groupedSource,
				IsGrouped = true
			};

			var page = new ContentPage() { Title = Tab3, Padding = 40 };

			page.Content = cv;

			return page;
		}

#if UITEST
		[Test]
		[FailsOnMauiAndroid]
		public void AddingItemToUnviewedCollectionViewShouldNotCrash()
		{
			RunningApp.WaitForElement(Add1);
			RunningApp.Tap(Add1);	
			RunningApp.Tap(Tab2);		

			RunningApp.WaitForElement(Success);
		}

		[Test]
		[FailsOnMauiAndroid]
		public void AddingGroupToUnviewedGroupedCollectionViewShouldNotCrash()
		{
			RunningApp.WaitForElement(Add2);
			RunningApp.Tap(Add2);
			RunningApp.Tap(Tab3);

			RunningApp.WaitForElement(Success);
		}
#endif
	}
}
