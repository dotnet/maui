namespace CollectionViewMemoryLeaks;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();

        Routing.RegisterRoute(nameof(Views.MainPage), typeof(Views.MainPage));
        Routing.RegisterRoute(nameof(Views.CollectionViewSamplePage), typeof(Views.CollectionViewSamplePage));
    }
}

