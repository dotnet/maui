namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	string[] pickerItems = new string[] { "Item 1", "Item 2", "Item 3" };

	public MainPage()
	{
		InitializeComponent();
		this.BindingContext = new BugViewModel();
		myPicker.ItemsSource = pickerItems;
	}


}
