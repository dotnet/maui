namespace MauiApp._1.Pages;

public partial class ProjectListPage : ContentPage
{
	public ProjectListPage(ProjectListPageModel model)
	{
		BindingContext = model;
		InitializeComponent();
		AppearingBehavior.BindingContext = model;
	}
}