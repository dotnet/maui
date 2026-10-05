namespace Bielik;

public partial class App : Application
{
    private readonly Func<AppShell> _createShell;

    public App(Func<AppShell> createShell)
    {
        InitializeComponent();
        UserAppTheme = AppTheme.Light;
        _createShell = createShell;
    }

    protected override Window CreateWindow(IActivationState? activationState) => new(_createShell());
}
