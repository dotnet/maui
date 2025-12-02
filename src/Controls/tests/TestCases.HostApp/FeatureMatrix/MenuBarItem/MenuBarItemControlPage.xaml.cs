namespace Maui.Controls.Sample;

public partial class MenuBarItemControlPage : ContentPage
{
	public MenuBarItemControlPage()
	{
		InitializeComponent();
		BindingContext = new MenuBarItemViewModel();
	}
}