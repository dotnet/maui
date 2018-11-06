using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 41271, "[UWP] Memory Leak from ListView in TabbedPage")]
	public class Bugzilla41271 : TestTabbedPage
	{
		[Preserve(AllMembers = true)]
		class Person
		{
			public Person(string firstName, string lastName, string city, string state)
			{
				FirstName = firstName;
				LastName = lastName;
				City = city;
				State = state;
			}
			public string FirstName { get; set; }
			public string LastName { get; set; }
			public string City { get; set; }
			public string State { get; set; }
		}
		[Preserve(AllMembers = true)]
		class ListViewCell : ViewCell
		{
			Label firstNameLabel = new Label();
			Label lastNameLabel = new Label();
			Label cityLabel = new Label();
			Label stateLabel = new Label();

			public ListViewCell()
			{
				View = new StackLayout
				{
					Orientation = StackOrientation.Horizontal,
					Children =
					{
						firstNameLabel,
						lastNameLabel,
						cityLabel,
						stateLabel
					}
				};
			}

			protected override void OnBindingContextChanged()
			{
				base.OnBindingContextChanged();
				var item = BindingContext as Person;
				if (item != null)
				{
					firstNameLabel.Text = item.FirstName;
					lastNameLabel.Text = item.LastName;
					cityLabel.Text = item.City;
					stateLabel.Text = item.State;
				}
			}
		}
		[Preserve(AllMembers = true)]
		class ListViewPage : ContentPage
		{
			ListView _ListView;
			List<Person> _People = new List<Person>();

			public ListViewPage(string id)
			{
				Title = $"List {id}";

				for (var x = 0; x < 1000; x++)
				{
					_People.Add(new Person("Bob", "Bobson", "San Francisco", "California"));
				}

				_ListView = new ListView(ListViewCachingStrategy.RecycleElement) { ItemTemplate = new DataTemplate(typeof(ListViewCell)) };
				Content = _ListView;
			}

			protected override void OnAppearing()
			{
				base.OnAppearing();

				_ListView.ItemsSource = _People;
			}

			protected override void OnDisappearing()
			{
				base.OnDisappearing();

				_ListView.ItemsSource = null;
			}
		}

		protected override void Init()
		{
			var counter = 1;

			for (var x = 0; x < 10; x++)
			{
				Children.Add(new ListViewPage(counter.ToString()));
				counter++;
			}
		}
	}
}
