using Recipes.Views;

namespace Recipes.ViewModels
{
    public class StartingPageViewModel : BaseViewModel
    {
        RestService _restService;

        string _searchQuery;

        public Command SearchCommand { get; }
        public Command<string> FilteredSearchCommand { get; }
        public Command<Hit> BalancedMealsTapped { get; }

        public StartingPageViewModel()
        {
            Title = "Recipes";
            _restService = new RestService();

            SearchCommand = new Command(async () => await OnSearch());
            FilteredSearchCommand = new Command<string>(async (filter) => await OnSearch(filter));
        }

        public RecipeData RecipeData { get; set; }

        public string SearchQuery
        {
            get => _searchQuery;
            set => SetProperty(ref _searchQuery, value);
        }

		async Task OnSearch(string filter = null)
        {
            // Require query and/or filter to search
            if (!string.IsNullOrWhiteSpace(SearchQuery) || !string.IsNullOrWhiteSpace(filter))
            {
                RecipeData recipeData = await _restService.GetRecipeDataAsync(GenerateRequestUri(Constants.EdamamEndpoint, filter));

                string urlEncodedFilter = System.Net.WebUtility.UrlEncode(filter);
                await Shell.Current.GoToAsync($"{nameof(SearchResultsPage)}?SearchQuery={SearchQuery}&SearchFilter={urlEncodedFilter}");

                SearchQuery = string.Empty;
            }
        }

        string GenerateRequestUri(string endpoint, string filter)
        {
            string requestUri = endpoint;

            if (string.IsNullOrEmpty(SearchQuery) && !string.IsNullOrEmpty(filter))
            {
                requestUri += $"?q={filter.Substring(filter.IndexOf("=") + 1)}";
            }
            else
            {
                requestUri += $"?q={SearchQuery}";
            }

            requestUri += $"&app_id={Constants.EdamamAppId}";
            requestUri += $"&app_key={Constants.EdamamAppKey}";

            if (!string.IsNullOrEmpty(filter))
            {
                requestUri += $"&{filter}";
            }

            return requestUri;
        }
    }
}
