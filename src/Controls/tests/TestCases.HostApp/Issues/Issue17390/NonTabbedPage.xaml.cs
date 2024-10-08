namespace Maui.Controls.Sample.Issues;

public partial class NonTabbedPage : ContentPage
{
	public NonTabbedPage()
	{
		InitializeComponent();
	}

    void BackToTabbedPage(object sender, EventArgs args)
	{
		 Navigation.PopAsync();
	}
}
