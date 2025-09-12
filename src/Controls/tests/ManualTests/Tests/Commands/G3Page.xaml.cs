using System.Windows.Input;

namespace Microsoft.Maui.ManualTests.Tests.Commands;

public partial class G3Page : ContentPage
{
	public G3Page()
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
