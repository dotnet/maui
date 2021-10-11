using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Recipes.ViewModels;
using System.Linq;

namespace Recipes.Views
{
    public partial class RecipeSearchPage : ContentPage
    {
        RecipeSearchViewModel _viewModel;

        public RecipeSearchPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new RecipeSearchViewModel();

			_viewModel.PropertyChanged += OnPropertedChanged;

		}

		private void OnImageHandlerChanged(object sender, System.EventArgs e)
		{
#if ANDROID
			if (sender is IView view)
			{
				if (view.Handler?.NativeView is Android.Widget.ImageView aView)
				{

				}
				else if(view.Handler?.NativeView is Android.Views.ViewGroup vg)
				{
					vg.SetClipChildren(true);
				}
			}
#endif
		}

		private void RecipeSearchPage_Tapped(object sender, System.EventArgs e)
		{
			BindableObject bo = sender as BindableObject;
			_viewModel.ItemTapped.Execute(bo.BindingContext);

		}

		//private void RecipeSearchPage_SelectedItemsChanged(object sender, Microsoft.Maui.SelectedItemsChangedEventArgs e)
		//{
		//	foreach(var item in e.NewSelection)
		//	{
		//		vListView.SetDeselected(vListView.SelectedItems.ToArray());
		//		_viewModel.SelectedHit = _viewModel.RecipeData.Hits[item.ItemIndex];
		//	}

		//}

		private void OnPropertedChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(RecipeSearchViewModel.RecipeData))
			{
				vListView.InvalidateData();
			}
		}

		protected override void OnAppearing()
        {
			_viewModel.SelectedHit = null;
			vListView.SetDeselected(vListView.SelectedItems.ToArray());
			base.OnAppearing();
            _viewModel.SearchCommand.Execute(null);
        }
    }
}