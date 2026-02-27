using System.Windows.Input;
using Microsoft.Maui.ManualTests.Categories;

namespace Microsoft.Maui.ManualTests.Tests.RefreshView;

[Test(
	id: "C7",
	title: "Button within RefreshView usable while refreshing.",
	category: Category.RefreshView)]
public partial class C7 : ContentPage
{
	public C7()
	{
		InitializeComponent();

		RefreshCommand = new Command(HandleRefreshCommand);
		Counter = 0;

		BindingContext = this;
	}

	private void HandleRefreshCommand()
	{
		Task.Run(async () =>
		{
			await Task.Delay(10000);
			RefreshView.IsRefreshing = false;
		});
	}

	public ICommand RefreshCommand { get; set; }

	public int Counter { get; set; }

	private void Button_Clicked(object sender, EventArgs e)
	{
		Counter++;

		if (Counter == 1)
			Button.Text = $"Clicked {Counter} time";
		else
			Button.Text = $"Clicked {Counter} times";
	}
}
