namespace MauiApp._1;

public static class Program
{
	// GDK on Windows needs a single-threaded apartment; the attribute is ignored on Linux.
	[STAThread]
	public static int Main(string[] args) => new GtkMauiApp().Run(args);
}

// The GTK 4 host: builds the MauiApp in "startup" and creates the first window in "activate".
sealed class GtkMauiApp : MauiGtkApplication
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
