using Microsoft.Maui.ManualTests.Categories;

namespace Microsoft.Maui.ManualTests.Tests.TitleBar;

[Test(
	id: "J2",
	title: "Check Window.TitleBar when push Modal Page.",
	category: Category.TitleBar)]
public partial class J2 : ContentPage
{
	public static int ModalPageCount2 = 0;
	public static string ModalStackCountText2 => $"ModalPageCount: {ModalPageCount2}";

	public J2()
	{
		InitializeComponent();
		BackgroundColor = Colors.White;
		Title = $"Modal Page {ModalPageCount2}";
	}

#if NET9_0_OR_GREATER
	Microsoft.Maui.Controls.TitleBar TitleBar = new Microsoft.Maui.Controls.TitleBar()
	{
		Title = "[TitleBar.TitleBar]",
		Subtitle = "[Title.Subtitle]",
		BackgroundColor = Color.FromRgb(255, 255, 0)
	};

	protected override void OnNavigatedTo(NavigatedToEventArgs args)
	{
		if (this.Window is null)
			return;

		this.Window.TitleBar = TitleBar;
	}

	protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
	{
		base.OnNavigatedFrom(args);

		var window = Application.Current.Windows.FirstOrDefault();

		if (window != null && Navigation.ModalStack.Count == 0)
		{
			window.TitleBar = null;
		}
	}
#endif

	private async void PushModalClicked(object sender, EventArgs e)
	{
		ModalPageCount2++;
		Page pushMe = new J2()
		{
			BackgroundColor =
			   (BackgroundColor == Colors.White) ? Colors.Pink : Colors.White
		};

		await Navigation.PushModalAsync(pushMe);
	}

	private async void PopModalClicked(object sender, EventArgs e)
	{
		if (Navigation.ModalStack.Count <= 0)
		{
			await DisplayAlert("Alert", "There is no more page to pop up, please click button 'Push Modal Page' to push page first !", "OK");
			return;
		}
		await Navigation.PopModalAsync(false);
		ModalPageCount2--;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		PopModal.IsVisible = Navigation.ModalStack.Count > 0;
	}
}