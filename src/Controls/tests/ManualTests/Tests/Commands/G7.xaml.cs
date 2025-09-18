using System.Windows.Input;
using Microsoft.Maui.ManualTests.Categories;

namespace Microsoft.Maui.ManualTests.Tests.Commands;

[Test(id: "G7", title: "Render Menuitems in the Shell as disabled if a command's CanExecute returns false.", category: Category.Commands)]
public partial class G7 : ContentPage
{
	public G7()
	{
		InitializeComponent();

		MenuCommand = new Command(execute: HandleMenuCommand, canExecute: () => false);

		BindingContext = this;
	}

	public ICommand MenuCommand { get; set; }

	private void HandleMenuCommand()
	{
		Console.WriteLine(nameof(HandleMenuCommand));
	}
}
