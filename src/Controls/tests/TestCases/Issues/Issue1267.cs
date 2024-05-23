using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	public class PersonCell : ViewCell
	{
		public PersonCell()
		{
			var grid = new Grid
			{
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
			grid.Add(label = new Label { BackgroundColor = Colors.Lime });
			label.SetBinding(Label.TextProperty, "FirstName");

			grid.Add(label = new Label(), 0, 1);
			label.SetBinding(Label.TextProperty, "LastName");

			grid.Add(label = new Label { HorizontalTextAlignment = TextAlignment.End }, 1, 0);
			label.SetBinding(Label.TextProperty, "Zip");

			grid.Add(label = new Label { VerticalTextAlignment = TextAlignment.End }, 1, 1);
			label.SetBinding(Label.TextProperty, "City");
			View = grid;


		}
	}

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1267, "Star '*' in Grid layout throws exception", PlatformAffected.WinPhone)]
	public class Issue1267 : TestContentPage
	{
		const string Success = "If this is visible, the test has passed.";

		protected override void Init()
		{
			var instructions = new Label { Text = Success };

			var lv = new ListView
			{
				ItemsSource = new[]{
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
				ItemTemplate = new DataTemplate(typeof(PersonCell)),
			};

			Content = new StackLayout { Children = { instructions, lv } };
		}
	}
}

