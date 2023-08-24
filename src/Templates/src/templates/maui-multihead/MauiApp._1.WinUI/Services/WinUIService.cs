using MauiApp._1.Services;

namespace MauiApp._1.WinUI.Services;

public class WinUIService : IPlatformSpecificService
{
	public WinUIService()
	{
	}

	public string GetPlatformDescription()
	{
		return "WinUI";
	}
}
