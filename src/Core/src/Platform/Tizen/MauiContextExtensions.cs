using Microsoft.Extensions.DependencyInjection;
using Tizen.NUI;

namespace Microsoft.Maui.Platform
{
	internal static partial class MauiContextExtensions
	{
		public static Window GetPlatformWindow(this IMauiContext mauiContext) =>
			mauiContext.Services.GetRequiredService<Window>();
	}
}
