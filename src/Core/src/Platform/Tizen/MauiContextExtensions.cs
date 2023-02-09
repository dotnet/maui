using Microsoft.Extensions.DependencyInjection;
using Tizen.NUI;
using Tizen.UIExtensions.NUI;

namespace Microsoft.Maui.Platform
{
	internal static partial class MauiContextExtensions
	{
		public static Window GetPlatformWindow(this IMauiContext mauiContext) =>
			mauiContext.Services.GetRequiredService<Window>();

		public static NavigationStack GetModalStack(this IMauiContext mauiContext) =>
			mauiContext.GetPlatformWindow().GetModalStack()!;

		public static IToolbarContainer? GetToolbarContainer(this IMauiContext mauiContext)
		{
			var modalStack = mauiContext.GetModalStack();
			foreach (var page in modalStack.Stack)
			{
				var realPage = page;
				if (page is ContainerView containerView)
				{
					realPage = containerView.CurrentPlatformView!;
				}

				if (realPage is IToolbarContainer toolbarContainer)
				{
					return toolbarContainer;
				}
			}
			return null;
		}
	}
}
