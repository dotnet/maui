using Recipes.Models;
using Recipes.ViewModels;

namespace Recipes.Views
{
    public partial class NewRecipePage : ContentPage
    {
        public Item Item { get; set; }

        public NewRecipePage()
        {
            InitializeComponent();
            BindingContext = new NewRecipeViewModel();
        }

		private async void OnValueChanged(object sender, ValueChangedEventArgs e)
		{
			string increasedOrDecreased = "";

			if (e.NewValue > e.OldValue)
				increasedOrDecreased = "Rating increased to ";
			else
				increasedOrDecreased = "Rating decreased to ";

			await Task.Delay(100);
			SemanticScreenReader.Announce(increasedOrDecreased + SemanticProperties.GetDescription(ratingLabel));
		}
	}
}
