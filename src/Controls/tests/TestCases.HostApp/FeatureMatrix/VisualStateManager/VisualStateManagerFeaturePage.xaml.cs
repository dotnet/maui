namespace Maui.Controls.Sample;

public class VisualStateManagerFeaturePage : NavigationPage
{
	public VisualStateManagerFeaturePage()
	{
		PushAsync(new VisualStateManagerFeatureMainPage());
	}
}
public partial class VisualStateManagerFeatureMainPage : ContentPage
{
	public VisualStateManagerFeatureMainPage()
	{
		InitializeComponent();
	}

	private async void OnVSMButtonClicked (object sender, EventArgs e)
	{
		await Navigation.PushAsync(new VisualStateManagerButtonPage ());
	}

	private async void OnVSMCheckBoxClicked (object sender, EventArgs e)
	{
		await Navigation.PushAsync(new VisualStateManagerCheckBoxPage ());
	}

	private async void OnVSMCollectionViewClicked (object sender, EventArgs e)
	{
		await Navigation.PushAsync(new VisualStateManagerCollectionViewPage ());
	}

	private async void OnVSMEntryClicked (object sender, EventArgs e)
	{
		await Navigation.PushAsync(new VisualStateManagerEntryPage ());
	}

	private async void OnVSMLabelClicked (object sender, EventArgs e)
	{
		await Navigation.PushAsync(new VisualStateManagerLabelPage ());
	}

	private async void OnVSMSliderClicked (object sender, EventArgs e)
	{
		await Navigation.PushAsync(new VisualStateManagerSliderPage ());
	}	

	private async void OnVSMSwitchClicked (object sender, EventArgs e)
	{
		await Navigation.PushAsync(new VisualStateManagerSwitchPage ());
	}
}