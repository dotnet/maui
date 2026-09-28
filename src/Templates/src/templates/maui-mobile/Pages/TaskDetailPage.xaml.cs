namespace MauiApp._1.Pages;

public partial class TaskDetailPage : ContentPage
{
	public TaskDetailPage(TaskDetailPageModel model)
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