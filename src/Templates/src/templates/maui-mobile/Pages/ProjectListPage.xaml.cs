//-:cnd:noEmit
#if WINDOWS
using MauiApp._1.Behaviors;
#endif
//+:cnd:noEmit
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
