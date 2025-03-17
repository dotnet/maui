namespace Maui.Controls.Sample.Issues;

public partial class Issue7856_1 : ContentPage
{
	public Issue7856_1()
	{
		InitializeComponent();

		Shell.SetBackButtonBehavior(this, new BackButtonBehavior
		{
			TextOverride = "Test"
		});
	}

	private void Navigate_Clicked(object sender, EventArgs e)
	{
		_ = Shell.Current.Navigation.PushAsync(new Issue7856_1());
	}
}
