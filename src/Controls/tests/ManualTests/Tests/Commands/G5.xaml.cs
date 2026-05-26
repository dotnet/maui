using System.Windows.Input;
using Microsoft.Maui.ManualTests.Categories;

namespace Microsoft.Maui.ManualTests.Tests.Commands;

[Test(id: "G5", title: "Command.ChangeCanExecute() does not work correctly.", category: Category.Commands)]
public partial class G5 : ContentPage
{
	public G5()
	{
		InitializeComponent();

		OnCommand = new Command(execute: HandleCommand, canExecute: () => !Toggle);
		OffCommand = new Command(execute: HandleCommand, canExecute: () => Toggle);

		BindingContext = this;
	}

	private void HandleCommand()
	{
		Toggle = !Toggle;
		((Command)OnCommand).ChangeCanExecute();
		((Command)OffCommand).ChangeCanExecute();
	}

	public bool Toggle { get; set; } = false;

	public ICommand OnCommand { get; set; }

	public ICommand OffCommand { get; set; }
}
