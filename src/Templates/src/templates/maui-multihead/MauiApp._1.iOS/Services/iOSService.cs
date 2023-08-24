using MauiApp._1.Services;

namespace MauiApp._1.iOS.Services;

public class iOSService : IPlatformSpecificService
{
	public iOSService()
	{
	}

	public string GetPlatformDescription()
	{
		return "iOS";
	}
}
