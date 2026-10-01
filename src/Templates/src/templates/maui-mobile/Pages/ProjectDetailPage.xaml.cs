using MauiApp._1.Models;
using MauiApp._1.Behaviors;

namespace MauiApp._1.Pages;

public partial class ProjectDetailPage : ContentPage
{
	public ProjectDetailPage(ProjectDetailPageModel model)
	{
		InitializeComponent();

		BindingContext = model;
//-:cnd:noEmit
#if WINDOWS
		TagsCollectionView.Behaviors.Add(new SingleSelectionKeyboardGuardBehavior());
		ProjectTasksCollectionView.Behaviors.Add(new SingleSelectionKeyboardGuardBehavior());
		IconsCollectionView.Behaviors.Add(new SingleSelectionKeyboardGuardBehavior());
#endif
//+:cnd:noEmit
	}
}
