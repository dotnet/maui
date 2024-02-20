using Maui.Controls.Legacy.Sample.Services;

namespace Maui.Controls.Legacy.Sample.Android.Services;

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
