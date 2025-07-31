using Recipes.ViewModels;

namespace Recipes.Views
{
	public partial class SearchResultsPage : ContentPage
    {
        SearchResultsViewModel _viewModel;

        public SearchResultsPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new SearchResultsViewModel();
			this.Loaded += (_, _) => searchBar.SetSemanticFocus();
        }

		private void OnImageHandlerChanged(object sender, System.EventArgs e)
		{
#if ANDROID
			if (sender is IView view)
			{
				if (view.Handler?.PlatformView is Android.Widget.ImageView aView)
				{

				}
				else if(view.Handler?.PlatformView is Android.Views.ViewGroup vg)
				{
					vg.SetClipChildren(true);
				}
			}
#endif
		}

		void SearchResultsPage_Tapped(object sender, System.EventArgs e)
		{
			BindableObject bo = sender as BindableObject;
			_viewModel.ItemTapped.Execute(bo.BindingContext);

		}

		protected override void OnAppearing()
        {
			_viewModel.SelectedHit = null;
			base.OnAppearing();
            _viewModel.SearchCommand.Execute(null);
        }
    }
}