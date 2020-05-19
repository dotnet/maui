using System;
using System.Collections.ObjectModel;

using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 2259, "ListView.ScrollTo crashes app", PlatformAffected.iOS)]
	public class Issue2259 : TestContentPage
	{
		[Preserve (AllMembers = true)]
		public class Person
		{
			public string Name { private set; get; }

			public DateTime Birthday { private set; get; }

			public Color FavoriteColor { private set; get; }

			public Person (string name, DateTime birthday, Color favoriteColor)
			{
				Name = name;
				Birthday = birthday;
				FavoriteColor = favoriteColor;
			}
		};

		int _count = 1;

		protected override void Init ()
		{
			var people = new ObservableCollection<Person> {
				new Person ("Abigail", new DateTime (1975, 1, 15), Color.Aqua),
				new Person ("Bob", new DateTime (1976, 2, 20), Color.Black),
				new Person ("Cathy", new DateTime (1977, 3, 10), Color.Blue),
#pragma warning disable 618
				new Person ("David", new DateTime (1978, 4, 25), Color.Fuschia),
#pragma warning restore 618
			};

			var buttonAdd = new Button {
				Text = "Add",
				HorizontalOptions = LayoutOptions.Start,
				VerticalOptions = LayoutOptions.Center,
			};

			var buttonRemove = new Button {
				Text = "Remove",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
			};

			var buttonScrollToBottom = new Button {
				Text = "Bottom",
				HorizontalOptions = LayoutOptions.Start,
				VerticalOptions = LayoutOptions.Center,
			};

			var buttonStack = new StackLayout {
				Orientation = StackOrientation.Horizontal,
				Children = {
					buttonAdd,
					buttonRemove,
					buttonScrollToBottom,
				}
			};

			var listView = new ListView {
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
				ItemsSource = people,
				ItemTemplate = new DataTemplate (() =>
				{
					var nameLabel = new Label ();
					var birthdayLabel = new Label ();
					var boxView = new BoxView ();

					var stack = new StackLayout {
						Padding = new Thickness (0, 5),
						Orientation = StackOrientation.Horizontal,
						BackgroundColor = Color.Black,
						Children = {
							boxView,
							new StackLayout {
								VerticalOptions = LayoutOptions.Center,
								Spacing = 0,
								Children = {
									nameLabel,
									birthdayLabel
								}
							}
						}
					};

					nameLabel.SetBinding (Label.TextProperty, "Name");
					birthdayLabel.SetBinding (Label.TextProperty, new Binding ("Birthday", BindingMode.OneWay, null, null, "Born {0:d}"));
					boxView.SetBinding (BoxView.ColorProperty, "FavoriteColor");
					stack.SetBinding (BackgroundColorProperty, "BackgroundColor");

					return new ViewCell {
						View = stack
					};
				})
			};

			buttonAdd.Clicked += (sender, e) =>
			{
				var person = new Person (string.Format ("Name {0}", _count++), DateTime.Today, Color.Blue);

				people.Add (person);

				listView.ScrollTo (person, ScrollToPosition.End, true);

			};

			buttonRemove.Clicked += (sender, e) => people.RemoveAt (people.Count - 1);

			buttonScrollToBottom.Clicked += (sender, e) =>
			{
				var person = people[people.Count - 1];

				listView.ScrollTo (person, ScrollToPosition.End, true);
			};

			Padding = Device.RuntimePlatform == Device.iOS ? new Thickness(10, 20, 10, 5) : new Thickness(10, 0, 10, 5);

			Content = new StackLayout {
				Orientation = StackOrientation.Vertical,
				Children = {
					buttonStack,
					listView,
				}
			};
		}
	
#if UITEST
		[Test]
		[UiTest (typeof(ListView), "ScrollTo")]
		public void Issue2259Tests ()
		{
			for (int i = 0; i < 20; i++) {
				RunningApp.Tap (q => q.Button ("Add"));
				RunningApp.WaitForElement (q => q.Marked ("Name " + (i + 1).ToString ()));
				RunningApp.Screenshot ("Added Cell");
			}
		}
#endif
	}
}


