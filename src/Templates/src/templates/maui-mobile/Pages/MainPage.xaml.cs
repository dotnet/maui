using MauiApp._1.Models;
using MauiApp._1.PageModels;

namespace MauiApp._1.Pages;

public partial class MainPage : ContentPage
{
	MainPageModel _model;
	public MainPage(MainPageModel model)
	{
		InitializeComponent();
		BindingContext = _model = model;
	}
}