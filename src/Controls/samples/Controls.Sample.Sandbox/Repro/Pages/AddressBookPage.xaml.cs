using AllTheLists.ViewModels;

namespace AllTheLists.Pages;

public partial class AddressBookPage : ContentPage
{
	public AddressBookPage()
	{
		InitializeComponent();
		BindingContext = new AddressBookViewModel();
	}
}