using Maui.Controls.Legacy.Sample.WinUI.Services;
using Maui.Controls.Legacy.Sample.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;

namespace Maui.Controls.Legacy.Sample;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();

		builder
			.UseSharedMauiApp();

		builder.Services.AddTransient<IPlatformSpecificService, WinUIService>();

		return builder.Build();
	}
}