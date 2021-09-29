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

			
			//vListView.SelectedItemsChanged += RecipeSearchPage_SelectedItemsChanged;

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