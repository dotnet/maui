using Recipes.ViewModels;

namespace Recipes.Views
{
    public partial class RecipeDetailPage : ContentPage
    {
        RecipeDetailViewModel _viewModel;

        public RecipeDetailPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new RecipeDetailViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }

        async void OpenUrl(object sender, EventArgs e)
        {
            SemanticScreenReader.Announce("Exiting app. Entering browser to view full recipe.");
            await Launcher.OpenAsync(_viewModel.RecipeUrl);
        }

        void Button_Loaded(object sender, EventArgs _)
        {
#if IOS || MACCATALYST
            if (sender is IElement e && e.Handler.PlatformView is UIKit.UIView uiView)
                uiView.AccessibilityTraits = UIKit.UIAccessibilityTrait.Link;
#endif
        }
    }
}