using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	public class PersonCell:ViewCell
	{
		public PersonCell ()
		{
			var grid = new Grid{ 
				RowDefinitions = new RowDefinitionCollection {
					new RowDefinition {Height = new GridLength (1, GridUnitType.Star)},
					new RowDefinition {Height = GridLength.Auto},
				},
				ColumnDefinitions = new ColumnDefinitionCollection {
					new ColumnDefinition {Width = new GridLength (1, GridUnitType.Star)},
					new ColumnDefinition {Width = GridLength.Auto},
				}
			};
			Label label;
			grid.Children.Add (label = new Label {BackgroundColor = Color.Lime});
			label.SetBinding (Label.TextProperty, "FirstName");			

			grid.Children.Add (label = new Label (),0,1);
			label.SetBinding (Label.TextProperty, "LastName");			

#pragma warning disable 618
			grid.Children.Add (label = new Label {XAlign = TextAlignment.End},1,0);
#pragma warning restore 618
			label.SetBinding (Label.TextProperty, "Zip");			

#pragma warning disable 618
			grid.Children.Add (label = new Label {XAlign = TextAlignment.End},1,1);
#pragma warning restore 618
			label.SetBinding (Label.TextProperty, "City");
			View = grid;

			
		}
	}

	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1267, "Star '*' in Grid layout throws exception", PlatformAffected.WinPhone)]
	public class Issue1267 : ContentPage
	{
		public Issue1267 ()
		{
			var lv = new ListView { 
				ItemsSource = new []{
					new {FirstName = "foo", LastName="bar", Zip="1234", City="Gotham City"},
					new {FirstName = "foo", LastName="bar", Zip="1234", City="Gotham City"},
					new {FirstName = "foo", LastName="bar", Zip="1234", City="Gotham City"},
					new {FirstName = "foo", LastName="bar", Zip="1234", City="Gotham City"},
					new {FirstName = "foo", LastName="bar", Zip="1234", City="Gotham City"},
					new {FirstName = "foo", LastName="bar", Zip="1234", City="Gotham City"},
					new {FirstName = "foo", LastName="bar", Zip="1234", City="Gotham City"},
					new {FirstName = "foo", LastName="bar", Zip="1234", City="Gotham City"},
					new {FirstName = "foo", LastName="bar", Zip="1234", City="Gotham City"},
					new {FirstName = "foo", LastName="bar", Zip="1234", City="Gotham City"},
					new {FirstName = "foo", LastName="bar", Zip="1234", City="Gotham City"},
					new {FirstName = "foo", LastName="bar", Zip="1234", City="Gotham City"},
					new {FirstName = "foo", LastName="bar", Zip="1234", City="Gotham City"},
				},
				ItemTemplate = new DataTemplate (typeof(PersonCell)),
			};
			Content = lv;
		}
	}
}

