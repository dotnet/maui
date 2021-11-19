using Microsoft.Extensions.DependencyInjection;
using ElmSharp;

namespace Microsoft.Maui
{
	internal static partial class MauiContextExtensions
	{
		
		public static Window GetNativeWindow(this IMauiContext mauiContext) =>
			mauiContext.Services.GetRequiredService<Window>();
	}
}
