using System.Windows.Input;
using Microsoft.Maui.ManualTests.Categories;

namespace Microsoft.Maui.ManualTests.Tests.Commands;

[Test(id: "G8", title: "ShellTitleView command binding not working.", category: Category.Commands)]
public partial class G8 : ContentPage
{
	public G8()
	{
		InitializeComponent();

		ImageButtonCommand = new Command(execute: HandleImageButtonCommandCommand);

		BindingContext = this;
	}

	public ICommand ImageButtonCommand { get; set; }

	private void HandleImageButtonCommandCommand()
	{
		Label.BackgroundColor = Colors.Green;
	}
}
