﻿using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 2259, "ListView.ScrollTo crashes app", PlatformAffected.iOS)]
	public class Issue2259 : TestContentPage
	{

		public class Person
		{
			public string Name { private set; get; }

			public DateTime Birthday { private set; get; }

			public Color FavoriteColor { private set; get; }

			public Person(string name, DateTime birthday, Color favoriteColor)
			{
				Name = name;
				Birthday = birthday;
				FavoriteColor = favoriteColor;
			}
		};

		int _count = 1;

		protected override void Init()
		{
			var people = new ObservableCollection<Person> {
				new Person ("Abigail", new DateTime (1975, 1, 15), Colors.Aqua),
				new Person ("Bob", new DateTime (1976, 2, 20), Colors.Black),
				new Person ("Cathy", new DateTime (1977, 3, 10), Colors.Blue),
#pragma warning disable 618
				new Person ("David", new DateTime (1978, 4, 25), Colors.Fuchsia),
#pragma warning restore 618
			};

			var buttonAdd = new Button
			{
				AutomationId = "AddButton",
				Text = "Add",
				HorizontalOptions = LayoutOptions.Start,
				VerticalOptions = LayoutOptions.Center,
			};

			var buttonRemove = new Button
			{
				AutomationId = "RemoveButton",
				Text = "Remove",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
			};

			var buttonScrollToBottom = new Button
			{
				AutomationId = "BottomButton",
				Text = "Bottom",
				HorizontalOptions = LayoutOptions.Start,
				VerticalOptions = LayoutOptions.Center,
			};

			var buttonStack = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				Children = {
					buttonAdd,
					buttonRemove,
					buttonScrollToBottom,
				}
			};

#pragma warning disable CS0618 // Type or member is obsolete
			var listView = new ListView
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
				ItemsSource = people,
				ItemTemplate = new DataTemplate(() =>
			   {
				   var nameLabel = new Label();
				   var birthdayLabel = new Label();
				   var boxView = new BoxView();

				   var stack = new StackLayout
				   {
					   Padding = new Thickness(0, 5),
					   Orientation = StackOrientation.Horizontal,
					   BackgroundColor = Colors.Black,
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

				   nameLabel.SetBinding(Label.AutomationIdProperty, "Name");
				   nameLabel.SetBinding(Label.TextProperty, "Name");
				   birthdayLabel.SetBinding(Label.TextProperty, new Binding("Birthday", BindingMode.OneWay, null, null, "Born {0:d}"));
				   boxView.SetBinding(BoxView.ColorProperty, "FavoriteColor");
				   stack.SetBinding(BackgroundColorProperty, "BackgroundColor");

				   return new ViewCell
				   {
					   View = stack
				   };
			   })
			};
#pragma warning restore CS0618 // Type or member is obsolete

			buttonAdd.Clicked += (sender, e) =>
			{
				var person = new Person(string.Format("Name {0}", _count++), DateTime.Today, Colors.Blue);

				people.Add(person);

				listView.ScrollTo(person, ScrollToPosition.End, true);

			};

			buttonRemove.Clicked += (sender, e) => people.RemoveAt(people.Count - 1);

			buttonScrollToBottom.Clicked += (sender, e) =>
			{
				var person = people[people.Count - 1];

				listView.ScrollTo(person, ScrollToPosition.End, true);
			};

			Padding = DeviceInfo.Platform == DevicePlatform.iOS ? new Thickness(10, 20, 10, 5) : new Thickness(10, 0, 10, 5);

			Content = new StackLayout
			{
				Orientation = StackOrientation.Vertical,
				Children = {
					buttonStack,
					listView,
				}
			};
		}
	}
}


