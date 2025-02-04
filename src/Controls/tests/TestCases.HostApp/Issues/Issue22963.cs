namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 22963, "Implementation of Customizable Search Icon Color for SearchBar Across Platforms", PlatformAffected.All)]

	public partial class Issue22963 : ContentPage
	{
		public Issue22963()
		{
			ConfigureSearchBar();
		}

		private void ConfigureSearchBar()
		{
			// Create the SearchBar  
			var searchBar = new SearchBar
			{
				Placeholder = "Search...",
				SearchIconColor = Colors.Magenta,
				WidthRequest = 300,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Start,
				AutomationId = "SearchBar"
			};
			// Add the SearchBar to the layout  
			var layout = new Grid();
			layout.Children.Add(searchBar);

			// Set the layout as the Content of the page  
			Content = layout;
		}
	}
}