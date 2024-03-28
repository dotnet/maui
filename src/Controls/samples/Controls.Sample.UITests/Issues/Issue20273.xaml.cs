using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 20273, "Previously selected item cannot be reselected", PlatformAffected.All)]

public partial class Issue20273 : ContentPage
{
	int _numberOfNavigations;

	public Issue20273()
	{
		InitializeComponent();
	}

	void CollectionView_SelectionChanged(System.Object sender, Microsoft.Maui.Controls.SelectionChangedEventArgs e)
	{
		Navigation.PushAsync(new Issue20273DetailPage());
		numberOfNavigationsLabel.Text = (++_numberOfNavigations).ToString();
	}
}

public class Issue20273DetailPage : ContentPage
{
	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await Task.Delay(100);
		await Navigation.PopAsync();
	}
}
