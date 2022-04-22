using ElmSharp;
using Microsoft.Extensions.DependencyInjection;
using ELayout = ElmSharp.Layout;

namespace Microsoft.Maui.Platform
{
	internal static partial class MauiContextExtensions
	{
		public static Window GetNativeWindow(this IMauiContext mauiContext) =>
			mauiContext.Services.GetRequiredService<Window>();

		public static ELayout GetNativeParent(this IMauiContext mauiContext) =>
			mauiContext.GetNativeWindow().GetBaseLayout();

		public static ModalStack GetModalStack(this IMauiContext mauiContext) =>
			mauiContext.GetNativeWindow().GetModalStack();
	}
}
