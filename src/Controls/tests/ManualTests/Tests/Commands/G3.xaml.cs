using System.Windows.Input;
using Microsoft.Maui.ManualTests.Categories;

namespace Microsoft.Maui.ManualTests.Tests.Commands;

[Test(id: "G3", title: "Page changing using menu bar stops command in menu bar from working.", category: Category.Commands)]
public partial class G3 : ContentPage
{
	public G3()
	{
		InitializeComponent();

		MenuCommand = new Command(HandleMenuCommand);

		Routing.RegisterRoute(nameof(G3Page), typeof(G3Page));

		BindingContext = this;
	}

	public ICommand MenuCommand { get; set; }

	private async void HandleMenuCommand()
	{
		await Shell.Current.GoToAsync(nameof(G3Page));
	}
}
