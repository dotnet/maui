using System.Windows.Input;
using Microsoft.Maui.ManualTests.Categories;

namespace Microsoft.Maui.ManualTests.Tests.Commands;

[Test(id: "G4", title: "Shell BackButtonBehavior binding Command to valid ICommand causes back button to disappear.", category: Category.Commands)]
public partial class G4 : ContentPage
{
	public G4()
	{
		InitializeComponent();

		GoBackCommand = new Command(HandleGoBackCommand);

		BindingContext = this;
	}

	public ICommand GoBackCommand { get; set; }

	private async void HandleGoBackCommand()
	{
		await Shell.Current.GoToAsync("..");
	}
}
