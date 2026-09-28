using MauiApp._1.Models;

namespace MauiApp._1.Pages;

public partial class ProjectDetailPage : ContentPage
{
	public ProjectDetailPage(ProjectDetailPageModel model)
	{
		InitializeComponent();

		BindingContext = model;
	}

	protected override void OnNavigatedTo(NavigatedToEventArgs args)
	{
		base.OnNavigatedTo(args);
		AppShell.UpdateBackButtonAccessibility(this);
	}
}
