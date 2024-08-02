namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
		vListView.ItemsSource = new List<Recipe> { 
			new Recipe(){ RecipeName="TJ1"}, 
			new Recipe(){ RecipeName="TJ2"}, 
			new Recipe(){ RecipeName="TJ3"}, 
			};
	}
}

public class Recipe
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
	public string RecipeName { get; set; }

	public string[] Ingredients { get; set; }

	public string ImageUrl { get; set; }

	public string RecipeUrl { get; set; }
}
