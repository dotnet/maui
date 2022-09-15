using Microsoft.Maui.Controls;

namespace MauiApp2;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
        Routing.RegisterRoute(nameof(Child), typeof(Child));
    }
}
