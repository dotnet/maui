using Microsoft.Maui.ManualTests.Categories;
using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Tests.TitleBar;

[Test(
	id: "J1",
	title: "Check Window.Title when push Modal Page.",
	category: Category.TitleBar)]
public partial class J1 : ContentPage
{
	static int ModalPageCount1 = 0;
	public static string ModalStackCountText1 => $"ModalPageCount: {ModalPageCount1}";

	string _previousTitle;

	public J1()
	{
		InitializeComponent();
		BackgroundColor = Colors.White;
		Title = $"Modal Page {ModalPageCount1}";
	}

	protected override void OnNavigatingFrom(NavigatingFromEventArgs args)
	{
		_previousTitle = this.Window?.Title;
		base.OnNavigatingFrom(args);
	}

	protected override void OnNavigatedTo(NavigatedToEventArgs args)
	{
		if (this.Window is null)
			return;

		if (PopModal.IsVisible)
		{
			this.Window.Title = "Modal Gallery";
		}
		else if (!String.IsNullOrWhiteSpace(_previousTitle))
		{
			this.Window.Title = _previousTitle;
		}
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		PopModal.IsVisible = Navigation.ModalStack.Count > 0;
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		this.Window.Title = "";
	}

	//async void PushNavigationModalClicked(object sender, EventArgs e)
	//{
	//    var j1 = new J1();
	//    Page pushMe = new NavigationPage(j1)
	//    {
	//        BackgroundColor =
	//                (BackgroundColor == Colors.White) ? Colors.Pink : Colors.White,
	//        Title = $"Navigation Root: {j1.Title}"
	//    };

	//    await Navigation.PushModalAsync(pushMe);

	//}

	async void PushModalClicked(object sender, EventArgs e)
	{
		ModalPageCount1++;

		Page pushMe = new J1()
		{
			BackgroundColor =
					   (BackgroundColor == Colors.White) ? Colors.Pink : Colors.White
		};

		await Navigation.PushModalAsync(pushMe);
	}

	//async void PushClicked(object sender, EventArgs e)
	//{
	//    await Navigation.PushAsync(new J1()
	//    {
	//        BackgroundColor =
	//            (BackgroundColor == Colors.White) ? Colors.Pink : Colors.White
	//    });
	//}

	async void PopModalClicked(object sender, EventArgs e)
	{
		if (Navigation.ModalStack.Count <= 0)
		{
			await DisplayAlert("Alert", "There is no more page to pop up, please click button 'Push Modal Page' to push page first !", "OK");
			return;
		}

		await Navigation.PopModalAsync();
		ModalPageCount1--;
	}

	//async void PushFlyoutPageClicked(object sender, EventArgs e)
	//{
	//    var j1 = new J1();
	//    Page newMainPage = new NavigationPage(j1)
	//    {
	//        BackgroundColor =
	//                (BackgroundColor == Colors.White) ? Colors.Pink : Colors.White,
	//        Title = $"Navigation Root: {j1.Title}"
	//    };

	//    var flyoutPage = new FlyoutPage()
	//    {
	//        Detail = newMainPage,
	//        Flyout = new ContentPage()
	//        {
	//            Content = new Label() { Text = "Flyout Text" },
	//            Title = "Flyout Title"
	//        }
	//    };

	//    await Navigation.PushModalAsync(flyoutPage);
	//}
}