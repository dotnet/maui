using Recipes.Services;

namespace Recipes;

public partial class App : Microsoft.Maui.Controls.Application
{

    public App()
    {
			InitializeComponent();
        DependencyService.Register<MockDataStore>();
#pragma warning disable CS0618 // Type or member is obsolete
		MainPage = new AppShell();
#pragma warning restore CS0618 // Type or member is obsolete
	}
}
