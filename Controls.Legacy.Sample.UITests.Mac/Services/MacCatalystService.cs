using Maui.Controls.Legacy.Sample.Services;

namespace Maui.Controls.Legacy.Sample.Mac.Services;

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
