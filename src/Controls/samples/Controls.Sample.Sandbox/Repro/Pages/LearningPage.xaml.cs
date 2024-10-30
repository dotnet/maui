using AllTheLists.ViewModels;

namespace AllTheLists.Pages;

public partial class LearningPage : ContentPage
{
	public LearningPage()
	{
		InitializeComponent();
		BindingContext = new LearningUnitsViewModel();
	}
}