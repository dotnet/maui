namespace Maui.Controls.Sample.Issues
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [Issue(IssueTracker.Github, 22963, "Implementation of Customizable Search Button Color for SearchBar Across Platforms", PlatformAffected.All)]

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
            searchBar.SearchButtonPressed += OnSearchButtonPressed;
            // Add the SearchBar to the layout  
            var layout = new Grid();  
            layout.Children.Add(searchBar);  

            // Set the layout as the Content of the page  
            Content = layout;  
        } 
        private async void OnSearchButtonPressed(object sender, EventArgs e)  
        {  
            // Display an alert when the search button is pressed  
            await DisplayAlert("Search Triggered", "You pressed the search button.", "OK");  
        }   
    }  
}