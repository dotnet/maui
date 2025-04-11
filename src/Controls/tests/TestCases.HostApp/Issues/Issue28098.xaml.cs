using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28098, "Returning back from navigation to MainPage would result in a blank screen", PlatformAffected.iOS)]
public partial class Issue28098 : ContentPage
{
	public Issue28098()
	{
		InitializeComponent();
#if ANDROID
		carouselView.HorizontalScrollBarVisibility = ScrollBarVisibility.Never;
#endif
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		_viewModel.OnAppearing();
	}
	private async void Button_Clicked(object sender, EventArgs e)
	{
		await Navigation.PushModalAsync(new ActionPage());
	}
}

public class ActionPage : ContentPage
{
	public ActionPage()
	{
		var button = new Button { Text = "GoBack", AutomationId = "BackButton" };
		button.Clicked += async (s, e) => await Navigation.PopModalAsync();
		Content = button;
	}
}

public class MainPageViewModel
{
	public Command LoadItemsCommand { get; }
	public ObservableCollection<Item> Items { get; }

	public void OnAppearing()
	{
		LoadItemsCommand.Execute(null);
	}

	void ExecuteLoadItemsCommand()
	{
		Items.Clear();
		Items.Add(new Item() { Name = "Item1" });
		Items.Add(new Item() { Name = "Item2" });
	}
	public MainPageViewModel()
	{
		Items = new ObservableCollection<Item>();

		LoadItemsCommand = new Command(() => ExecuteLoadItemsCommand());
	}
}

public class Item
{
	public string Name { get; set; }
}