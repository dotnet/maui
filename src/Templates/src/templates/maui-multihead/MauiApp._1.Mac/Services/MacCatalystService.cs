using MauiApp._1.Services;

namespace MauiApp._1.Mac.Services;

public class MacCatalystService : IPlatformSpecificService
{
	public MacCatalystService()
	{
	}

	public string GetPlatformDescription()
	{
		return "Mac Catalyst";
	}
}
