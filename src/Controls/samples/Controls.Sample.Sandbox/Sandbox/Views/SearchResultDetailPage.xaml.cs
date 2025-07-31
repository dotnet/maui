using Recipes.ViewModels;

namespace Recipes.Views
{
    public partial class SearchResultDetailPage : ContentPage
    {

        SearchResultDetailViewModel _viewModel;
        Color primary = Color.FromArgb("#176515");
        string alertMessage = "This recipe has been added to your personal recipe collection! Go to the 'My Recipes' tab to view and modify this recipe from there";

        public SearchResultDetailPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new SearchResultDetailViewModel();
        }


        async void OpenUrl(object sender, EventArgs e)
        {
            SemanticScreenReader.Announce("Exiting app. Entering browser to view full recipe.");
            await Launcher.OpenAsync(_viewModel.Hit.Recipe.RecipeUrl);
        }

        void Button_Loaded(object sender, EventArgs _)
        {
#if IOS || MACCATALYST
            if (sender is IElement e && e.Handler.PlatformView is UIKit.UIView uiView)
                uiView.AccessibilityTraits = UIKit.UIAccessibilityTrait.Link;
#endif
        }

        async void AddItem_Clicked(object sender, System.EventArgs e)
        {
			await Task.Delay(100);
			SemanticScreenReader.Announce(alertMessage);

            // TODO MAUI
            //var messageOptions = new MessageOptions
            //{
            //	Foreground = Color.White,
            //	Message = alertMessage,
            //	Font = Font.SystemFontOfSize(14),
            //	Padding = new Thickness(10)
            //};

            //var options = new SnackBarOptions
            //{
            //	MessageOptions = messageOptions,
            //	Duration = TimeSpan.FromMilliseconds(5000),
            //	BackgroundColor = primary,
            //	IsRtl = false
            //};

            //await this.DisplaySnackBarAsync(options);
        }
    }
}