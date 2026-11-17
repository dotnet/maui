using MauiApp._1.Models;
using MauiApp._1.PageModels;
//-:cnd:noEmit
#if WINDOWS
using MauiApp._1.Behaviors;
#endif
//+:cnd:noEmit

namespace MauiApp._1.Pages;

public partial class MainPage : ContentPage
{
	public MainPage(MainPageModel model)
	{
		InitializeComponent();
		BindingContext = model;
//-:cnd:noEmit
#if WINDOWS
		ProjectsCollectionView.Behaviors.Add(new SingleSelectionKeyboardGuardBehavior());
		TasksCollectionView.Behaviors.Add(new SingleSelectionKeyboardGuardBehavior());
#endif
//+:cnd:noEmit
	}
}
