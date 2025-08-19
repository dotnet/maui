using Microsoft.Maui.ManualTests.Categories;
using System.Windows.Input;

namespace Microsoft.Maui.ManualTests.Tests.RefreshView;

[Test(
    id: "C5",
    title: "Refresh command is executed correctly.",
    category: Category.RefreshView)]
public partial class C5 : ContentPage
{
	public C5()
	{
		InitializeComponent();

        RefreshCommand = new Command(HandleRefreshCommand);

        BindingContext = this;
    }

    public ICommand RefreshCommand { get; set; }

    private void HandleRefreshCommand()
    {
        RefreshView.IsRefreshing = false;
    }
}
