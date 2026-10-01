using MauiApp._1.Behaviors;
namespace MauiApp._1.Pages;

public partial class ProjectListPage : ContentPage
{
	public ProjectListPage(ProjectListPageModel model)
	{
		BindingContext = model;
		InitializeComponent();
//-:cnd:noEmit
#if WINDOWS
		ProjectsCollectionView.Behaviors.Add(new SingleSelectionKeyboardGuardBehavior());
#endif
//+:cnd:noEmit
	}
}
