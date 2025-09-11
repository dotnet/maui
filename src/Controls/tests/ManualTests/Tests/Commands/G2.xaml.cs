using System.Windows.Input;
using Microsoft.Maui.ManualTests.Categories;

namespace Microsoft.Maui.ManualTests.Tests.Commands;

[Test(id: "G2", title: "Entry Return command not firing up.", category: Category.Commands)]
public partial class G2 : ContentPage
{
	public G2()
	{
		InitializeComponent();

		Entry1Command = new Command(HandleEntry1Command);
		Entry2Command = new Command(HandleEntry2Command);
		Entry3Command = new Command(HandleEntry3Command);

		BindingContext = this;
	}

	public ICommand Entry1Command { get; set; }

	public ICommand Entry2Command { get; set; }

	public ICommand Entry3Command { get; set; }

	private void HandleEntry1Command()
	{
		Label1.BackgroundColor = Colors.Green;
	}

	private void HandleEntry2Command()
	{
		Label2.BackgroundColor = Colors.Green;
	}

	private void HandleEntry3Command()
	{
		Label3.BackgroundColor = Colors.Green;
	}
}
