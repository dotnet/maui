using Maui.Controls.Legacy.Sample.Services;

namespace Maui.Controls.Legacy.Sample.WinUI.Services;

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
