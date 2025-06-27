namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

	private void OnToggleClicked(object sender, EventArgs e)
	{
		ToggleableItem.IsVisible = !ToggleableItem.IsVisible;
		ToggleButton.Text = ToggleableItem.IsVisible ? "Hide 'Toggle Me' Item" : "Show 'Toggle Me' Item";
	}

	private void OnHideAllClicked(object sender, EventArgs e)
	{
		AlwaysVisibleItem.IsVisible = false;
		ToggleableItem.IsVisible = false;
		AnotherItem.IsVisible = false;
	}

	private void OnShowAllClicked(object sender, EventArgs e)
	{
		AlwaysVisibleItem.IsVisible = true;
		ToggleableItem.IsVisible = true;
		AnotherItem.IsVisible = true;
		ToggleButton.Text = "Hide 'Toggle Me' Item";
	}
}