namespace maui_blazor;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
		_ = CommonMethods.Invoke();
	}
}