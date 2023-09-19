using Recipes.Services;

namespace Recipes;

public partial class App : Microsoft.Maui.Controls.Application
{

    public App()
    {
			InitializeComponent();
        DependencyService.Register<MockDataStore>();
			MainPage = new AppShell();
		}
}
