using MauiApp._1.Services;

namespace MauiApp._1.Android.Services;

public class AndroidService : IPlatformSpecificService
{
	public AndroidService()
	{
	}

	public string GetPlatformDescription()
	{
		return "Android";
	}
}
