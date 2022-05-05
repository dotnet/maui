using ElmSharp;
using Microsoft.Extensions.DependencyInjection;
using ELayout = ElmSharp.Layout;

namespace Microsoft.Maui.Platform
{
	internal static partial class MauiContextExtensions
	{
		public static Window GetPlatformWindow(this IMauiContext mauiContext) =>
			mauiContext.Services.GetRequiredService<Window>();

		public static ELayout GetPlatformParent(this IMauiContext mauiContext) =>
			mauiContext.GetPlatformWindow().GetBaseLayout();

		public static ModalStack GetModalStack(this IMauiContext mauiContext) =>
			mauiContext.GetPlatformWindow().GetModalStack();
	}
}
