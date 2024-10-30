using System.Diagnostics;
using AllTheLists.Models;
using AllTheLists.Pages;
using AllTheLists.Views;

namespace AllTheLists;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

	private async void ListView_ItemSelected(object sender, EventArgs e)
	{
		// var selectedItem = (sender as ListView).SelectedItem;
		// if (selectedItem is Sample sampleModel)
		// {
		var tappedView = (sender as View);
		Sample itemData = (Sample)tappedView.BindingContext;
		if (itemData.Page != null)
		{
			await Navigation.PushAsync((ContentPage)Activator.CreateInstance(itemData.Page));
		}
		// }
	}

	async void Actions_Tapped(object sender, EventArgs e)
	{
		try{
			var sheet = new SampleBottomSheet();        
			await sheet.ShowAsync(App.Current.MainPage.Window);
		}catch(Exception ooops){
			Debug.WriteLine(ooops.Message);
		}
	}

	
}