using MauiApp._1.Models;
//-:cnd:noEmit
#if WINDOWS
using MauiApp._1.Behaviors;
#endif
//+:cnd:noEmit

namespace MauiApp._1.Pages;

public partial class ProjectDetailPage : ContentPage
{
	public ProjectDetailPage(ProjectDetailPageModel model)
	{
		InitializeComponent();

		BindingContext = model;
//-:cnd:noEmit
#if WINDOWS
		ProjectTasksCollectionView.Behaviors.Add(new SingleSelectionKeyboardGuardBehavior());
		IconsCollectionView.Behaviors.Add(new SingleSelectionKeyboardGuardBehavior());
#endif
//+:cnd:noEmit
	}
}
