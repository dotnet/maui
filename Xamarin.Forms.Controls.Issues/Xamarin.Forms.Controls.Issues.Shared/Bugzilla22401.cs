using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms.Controls.Issues;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Bugzilla, 22401, "MasterDetailPage detail width broken when landscape", PlatformAffected.iOS, NavigationBehavior.PushAsync)]
	public class Bugzilla22401 : MasterDetailPage
	{
		public Bugzilla22401()
		{
			List<Person> people = GetDemoData ();

			// Create the ListView.
			var listView = new ListView {
				// Source of data items.
				ItemsSource = people,

				// Define template for displaying each item.
				// (Argument of DataTemplate constructor is called for 
				//      each item; it must return a Cell derivative.)
				ItemTemplate = new DataTemplate (() => {
					// Create views with bindings for displaying each property.
					Label nameLabel = new Label();
					nameLabel.SetBinding (Label.TextProperty, "Name");

					// Return an assembled ViewCell.
					return new ViewCell {
						View = new StackLayout {
							Padding = new Thickness(0, 5),
							Orientation = StackOrientation.Horizontal,
							Children = {
								new StackLayout {
									VerticalOptions = LayoutOptions.Center,
									Spacing = 0,
									Children = {
										nameLabel
									}
								}
							}
						}
					};
				})
			};

			Master = new ContentPage { Title = "master", Icon = "menuIcon.png", Content = listView };

			listView.ItemSelected += (sender, e) => {
				Detail = CreateDetailPage (string.Format("Page {0}", (e.SelectedItem as Person).Name));
				IsPresented = false;
			};
			listView.SelectedItem = people.First ();
		}

		static List<Person> GetDemoData()
		{
			List<Person> people = new List<Person> {
				new Person("Abigail"),
				new Person("Bob"),
				new Person("Cathy"), 
				new Person("David"),
				new Person("Eugenie"),
				new Person("Freddie"),
				new Person("Greta"), 
				new Person("Harold"),
				new Person("Irene"),
				new Person("Jonathan"),
				new Person("Kathy"),
				new Person("Larry"),
				new Person("Monica"),
				new Person("Nick"),
				new Person("Olive"),
				new Person("Pendletonlow"),
				new Person("Queenie"),
				new Person("Rob"),
				new Person("Sally"),
				new Person("Timothy"),
				new Person("Uma"),
				new Person("Victor"),
				new Person("Wendy"),
				new Person("Xavier"),
				new Person("Yvonne"),
				new Person("Zachary"),
			};
			return people;
		}

		static Page CreateDetailPage(string text)
		{
			var page = new ContentPage {
				Title = text,
				Content = new StackLayout {
					Children = {
						new Label { 
							Text = text,
							VerticalOptions = LayoutOptions.CenterAndExpand,
							HorizontalOptions = LayoutOptions.CenterAndExpand,
						}
					}
				}
			};

			var tbiBank = new ToolbarItem { Command = new Command (() => { }), Icon = "bank.png" };
			var tbiCalc = new ToolbarItem { Command = new Command (() => { }), Icon = "calculator.png" };

			page.ToolbarItems.Add (tbiBank);
			page.ToolbarItems.Add (tbiCalc);

			return new NavigationPage (page);
		}
	}
}
