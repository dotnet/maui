using System.Windows.Input;
using Microsoft.Maui.ManualTests.Categories;

namespace Microsoft.Maui.ManualTests.Tests.Commands;

[Test(id: "G1", title: "Button IsEnabled property doesn't work if used with Command property.", category: Category.Commands)]
public partial class G1 : ContentPage
{
	public G1()
	{
		InitializeComponent();
	}

	private void Button_Clicked(object sender, EventArgs e)
	{
		Console.WriteLine("Button_Clicked");
	}
}

public class ViewModel
{
	public ViewModel()
	{
		Command = new Command(HandleCommand);
	}

	public bool IsButtonEnabled { get; set; } = false;

	public ICommand Command { get; set; }

	private static void HandleCommand()
	{
		Console.WriteLine("HandleCommand");
	}
}

