using Maui.Controls.Legacy.Sample.Services;

namespace Maui.Controls.Legacy.Sample.iOS.Services;

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
