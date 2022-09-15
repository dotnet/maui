using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace MauiApp2;

public partial class Child : ContentPage
{
	public Child(ChildModel model)
	{
		InitializeComponent();

		BindingContext = model;
	}
}

public partial class ChildModel
{
	public async Task Navigate()
	{
		await Shell.Current.GoToAsync("//HomePage");
	}
}