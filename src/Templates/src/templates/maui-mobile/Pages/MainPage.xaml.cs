using MauiApp._1.Models;
using MauiApp._1.PageModels;

namespace MauiApp._1.Pages;

public partial class MainPage : ContentPage
{
	public MainPage(MainPageModel model)
	{
		InitializeComponent();
		BindingContext = model;
	}
}