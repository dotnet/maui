namespace Maui.Controls.Sample.Issues;

public partial class InnerTabbedPage : ContentPage
{
	public InnerTabbedPage()
	{
		InitializeComponent();
	}

    async void OpenNonTabbedPage(object sender, EventArgs args)
    {
        await Shell.Current.GoToAsync("nontabbedpage");
    }
}
