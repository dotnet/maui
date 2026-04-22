using MauiApp._1.Resources.Styles;

namespace MauiApp._1;

public class App : Application
{
	public App()
	{
		var colors = new AppColors();
		Resources.MergedDictionaries.Add(colors);
		Resources.MergedDictionaries.Add(new AppStyles(colors));
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}
